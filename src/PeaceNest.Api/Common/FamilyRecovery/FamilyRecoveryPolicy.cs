using Microsoft.Extensions.Options;

namespace PeaceNest.Api.Common.FamilyRecovery;

public sealed class FamilyRecoveryPolicy
{
    private readonly FamilyRecoveryOptions _options;

    public FamilyRecoveryPolicy(IOptions<FamilyRecoveryOptions> options)
    {
        _options = options.Value;
    }

    public int BatchSize => _options.BatchSize;

    public bool WorkerEnabled => _options.WorkerEnabled;

    public TimeSpan SweepInterval => TimeSpan.FromMinutes(_options.SweepIntervalMinutes);

    public DateTimeOffset GetRecoveryExpiry(DateTimeOffset createdAt) =>
        createdAt.AddDays(_options.LifetimeDays);

    public DateTimeOffset GetStaleClaimBoundary(DateTimeOffset now) =>
        now.AddMinutes(-_options.ClaimLeaseMinutes);

    public bool IsExpired(DateTimeOffset expiresAt, DateTimeOffset now) => expiresAt <= now;
}
