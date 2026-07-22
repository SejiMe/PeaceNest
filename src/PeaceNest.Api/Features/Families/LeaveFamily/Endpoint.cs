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

namespace PeaceNest.Api.Features.Families.LeaveFamily;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly FamilyRecoveryCodeService _codeService;
    private readonly FamilyRecoveryPolicy _recoveryPolicy;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        PeaceNestDbContext dbContext,
        FamilyRecoveryCodeService codeService,
        FamilyRecoveryPolicy recoveryPolicy,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _codeService = codeService;
        _recoveryPolicy = recoveryPolicy;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families/{familyId:guid}/leave");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Families"));
        Summary(summary =>
        {
            summary.Summary = "Leave a family workspace.";
            summary.Description = "Removes the current membership. A sole creator receives a one-time recovery code and starts the 30-day deletion window.";
            summary.Responses[200] = "The member left the family workspace.";
            summary.Responses[403] = "The authenticated user is not an active family member.";
            summary.Responses[422] = "The owner must transfer ownership before leaving.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        var now = _timeProvider.GetUtcNow();

        IDbContextTransaction? transaction = null;
        if (_dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        }

        try
        {
            var family = await _dbContext.Families
                .SingleOrDefaultAsync(candidate => candidate.Id == familyId, ct)
                ?? throw new NotFoundAppException("Family workspace was not found.");
            var membership = await _dbContext.FamilyMembers
                .SingleOrDefaultAsync(candidate =>
                    candidate.FamilyId == familyId &&
                    candidate.UserId == user.Id &&
                    candidate.Status == FamilyMemberStatus.Active,
                    ct)
                ?? throw new AuthorizationAppException("You are not an active member of this family workspace.");

            if (membership.Role != FamilyMemberRole.Owner)
            {
                membership.Status = FamilyMemberStatus.Removed;
                membership.RemovedAt = now;
                await _dbContext.SaveChangesAsync(ct);
                if (transaction is not null)
                {
                    await transaction.CommitAsync(ct);
                }

                await Send.OkAsync(new Response(familyId, false, null, null), ct);
                return;
            }

            var activeMemberCount = await _dbContext.FamilyMembers.CountAsync(candidate =>
                candidate.FamilyId == familyId && candidate.Status == FamilyMemberStatus.Active,
                ct);
            if (activeMemberCount > 1)
            {
                throw new DomainRuleAppException("Transfer ownership before leaving this family workspace.");
            }

            if (family.CreatedByUserId != user.Id)
            {
                throw new DomainRuleAppException("Only the workspace creator can start the family recovery window.");
            }

            var plaintextCode = _codeService.GenerateCode();
            var recoveryCode = new FamilyRecoveryCode
            {
                FamilyId = familyId,
                Family = family,
                CreatorUserId = user.Id,
                CreatorUser = user,
                CodeHash = _codeService.Hash(plaintextCode),
                Status = FamilyRecoveryCodeStatus.Active,
                ExpiresAt = _recoveryPolicy.GetRecoveryExpiry(now)
            };

            var activeJoinCodes = await _dbContext.FamilyJoinCodes
                .Where(code => code.FamilyId == familyId && code.Status == FamilyJoinCodeStatus.Active)
                .ToListAsync(ct);
            foreach (var code in activeJoinCodes)
            {
                code.Status = FamilyJoinCodeStatus.Revoked;
                code.RevokedByUserId = user.Id;
                code.RevokedAt = now;
            }

            var pendingRequests = await _dbContext.FamilyJoinRequests
                .Where(request => request.FamilyId == familyId && request.Status == FamilyJoinRequestStatus.Pending)
                .ToListAsync(ct);
            foreach (var request in pendingRequests)
            {
                request.Status = FamilyJoinRequestStatus.Cancelled;
            }

            var pendingRequestIds = pendingRequests.Select(request => request.Id).ToArray();
            if (pendingRequestIds.Length > 0)
            {
                var staleNotifications = await _dbContext.Notifications
                    .Where(notification =>
                        notification.RelatedJoinRequestId != null &&
                        pendingRequestIds.Contains(notification.RelatedJoinRequestId.Value))
                    .ToListAsync(ct);
                foreach (var notification in staleNotifications)
                {
                    notification.DeletedAt = now;
                }
            }

            membership.Status = FamilyMemberStatus.Removed;
            membership.RemovedAt = now;
            family.DeletedAt = now;
            _dbContext.FamilyRecoveryCodes.Add(recoveryCode);
            await _dbContext.SaveChangesAsync(ct);

            if (transaction is not null)
            {
                await transaction.CommitAsync(ct);
            }

            await Send.OkAsync(
                new Response(familyId, true, plaintextCode, recoveryCode.ExpiresAt),
                ct);
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
