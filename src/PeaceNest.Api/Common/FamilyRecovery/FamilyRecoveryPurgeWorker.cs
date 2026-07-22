using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.FamilyRecovery;

public sealed class FamilyRecoveryPurgeWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FamilyRecoveryPolicy _policy;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<FamilyRecoveryPurgeWorker> _logger;

    public FamilyRecoveryPurgeWorker(
        IServiceScopeFactory scopeFactory,
        FamilyRecoveryPolicy policy,
        TimeProvider timeProvider,
        ILogger<FamilyRecoveryPurgeWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _policy = policy;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_policy.WorkerEnabled)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Family recovery purge sweep failed.");
            }

            await Task.Delay(_policy.SweepInterval, _timeProvider, stoppingToken);
        }
    }

    internal async Task SweepAsync(CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        List<Guid> candidates;

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
            var staleClaimBoundary = _policy.GetStaleClaimBoundary(now);
            candidates = await dbContext.FamilyRecoveryCodes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(code =>
                    code.Status == FamilyRecoveryCodeStatus.Active &&
                    code.ExpiresAt <= now &&
                    (code.PurgeClaimedAt == null || code.PurgeClaimedAt <= staleClaimBoundary))
                .OrderBy(code => code.ExpiresAt)
                .Select(code => code.Id)
                .Take(_policy.BatchSize)
                .ToListAsync(cancellationToken);
        }

        foreach (var recoveryCodeId in candidates)
        {
            var familyId = await TryClaimAsync(recoveryCodeId, now, cancellationToken);
            if (familyId is null)
            {
                continue;
            }

            try
            {
                using var purgeScope = _scopeFactory.CreateScope();
                var purger = purgeScope.ServiceProvider.GetRequiredService<FamilyDataPurger>();
                await purger.PurgeExpiredFamilyAsync(familyId.Value, now, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to purge expired family workspace {FamilyId}.", familyId);
            }
        }
    }

    private async Task<Guid?> TryClaimAsync(
        Guid recoveryCodeId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PeaceNestDbContext>();
        var code = await dbContext.FamilyRecoveryCodes
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(candidate => candidate.Id == recoveryCodeId, cancellationToken);

        if (code is null ||
            code.Status != FamilyRecoveryCodeStatus.Active ||
            code.ExpiresAt > now ||
            (code.PurgeClaimedAt is not null && code.PurgeClaimedAt > _policy.GetStaleClaimBoundary(now)))
        {
            return null;
        }

        code.PurgeClaimedAt = now;
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return code.FamilyId;
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }
    }
}
