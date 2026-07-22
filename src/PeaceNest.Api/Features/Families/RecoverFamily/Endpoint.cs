using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.FamilyRecovery;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.Families.RecoverFamily;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly FamilyRecoveryCodeService _codeService;
    private readonly FamilyRecoveryPolicy _recoveryPolicy;
    private readonly FamilyDataPurger _familyDataPurger;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        PeaceNestDbContext dbContext,
        FamilyRecoveryCodeService codeService,
        FamilyRecoveryPolicy recoveryPolicy,
        FamilyDataPurger familyDataPurger,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _codeService = codeService;
        _recoveryPolicy = recoveryPolicy;
        _familyDataPurger = familyDataPurger;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families/recover");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Invite));
        Description(builder => builder.WithTags("Families"));
        Summary(summary =>
        {
            summary.Summary = "Recover an inactive family workspace.";
            summary.Description = "Restores the same authenticated creator as Owner when the single-use recovery code is valid and unexpired.";
            summary.Responses[200] = "The family workspace was recovered.";
            summary.Responses[403] = "The authenticated user is not the workspace creator.";
            summary.Responses[422] = "The recovery code is invalid, used, or expired.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || !_codeService.IsValid(request.Code))
        {
            throw new ValidationAppException(
                "Family recovery request is invalid.",
                [new ValidationFailure("code", "Enter a valid 20-character family recovery code.")]);
        }

        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        CurrentUserService.RequireCompletedProfile(user);
        var now = _timeProvider.GetUtcNow();
        var codeHash = _codeService.Hash(request.Code);
        var recoveryCode = await _dbContext.FamilyRecoveryCodes
            .IgnoreQueryFilters()
            .Include(code => code.Family)
            .SingleOrDefaultAsync(code => code.CodeHash == codeHash, ct);

        if (recoveryCode is null || recoveryCode.Status != FamilyRecoveryCodeStatus.Active)
        {
            throw new DomainRuleAppException("This family recovery code is invalid or no longer available.");
        }

        if (recoveryCode.CreatorUserId != user.Id || recoveryCode.Family.CreatedByUserId != user.Id)
        {
            throw new AuthorizationAppException("Only the original workspace creator can use this recovery code.");
        }

        if (_recoveryPolicy.IsExpired(recoveryCode.ExpiresAt, now))
        {
            var familyId = recoveryCode.FamilyId;
            _dbContext.ChangeTracker.Clear();
            await _familyDataPurger.PurgeExpiredFamilyAsync(familyId, now, ct);
            throw new DomainRuleAppException("This family recovery window has expired and the workspace can no longer be restored.");
        }

        if (recoveryCode.Family.DeletedAt is null)
        {
            throw new DomainRuleAppException("This family workspace is already active.");
        }

        IDbContextTransaction? transaction = null;
        if (_dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        }

        try
        {
            var membership = await _dbContext.FamilyMembers.SingleAsync(member =>
                member.FamilyId == recoveryCode.FamilyId && member.UserId == user.Id,
                ct);
            membership.Role = FamilyMemberRole.Owner;
            membership.Status = FamilyMemberStatus.Active;
            membership.RemovedAt = null;
            recoveryCode.Family.DeletedAt = null;
            recoveryCode.Status = FamilyRecoveryCodeStatus.Used;
            recoveryCode.UsedAt = now;
            recoveryCode.PurgeClaimedAt = null;

            await _dbContext.SaveChangesAsync(ct);
            if (transaction is not null)
            {
                await transaction.CommitAsync(ct);
            }

            await Send.OkAsync(
                new Response(
                    recoveryCode.FamilyId,
                    recoveryCode.Family.Name,
                    recoveryCode.Family.PreferredCurrency,
                    FamilyMemberRole.Owner,
                    now),
                ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictAppException("The family recovery state changed. Please try again.");
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }
}
