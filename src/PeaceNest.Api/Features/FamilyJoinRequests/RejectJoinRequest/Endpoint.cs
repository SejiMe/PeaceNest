using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyJoinRequests.RejectJoinRequest;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _authorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer authorizer,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _authorizer = authorizer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/families/{familyId:guid}/join-requests/{requestId:guid}/reject");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Join Requests"));
        Summary(summary => summary.Summary = "Reject a pending family join request.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var requestId = Route<Guid>("requestId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var reviewer = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        await _authorizer.RequireActiveMembershipAsync(
            familyId,
            reviewer.Id,
            FamilyRolePermissions.CanReviewFamilyJoinRequests,
            "Only family owners and parent/admins can reject join requests.",
            ct);

        var now = _timeProvider.GetUtcNow();
        var joinRequest = await _dbContext.FamilyJoinRequests
            .Include(candidate => candidate.Family)
            .Include(candidate => candidate.RequesterUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.FamilyId == familyId, ct)
            ?? throw new NotFoundAppException("Family join request was not found.");

        if (joinRequest.Status == FamilyJoinRequestStatus.Rejected)
        {
            await Send.OkAsync(new Response(FamilyJoinRequestResponseProjection.FromRequest(joinRequest, now)), ct);
            return;
        }

        if (joinRequest.Status != FamilyJoinRequestStatus.Pending)
        {
            throw new DomainRuleAppException("Only a pending family join request can be rejected.");
        }

        joinRequest.Status = joinRequest.ExpiresAt <= now
            ? FamilyJoinRequestStatus.Expired
            : FamilyJoinRequestStatus.Rejected;
        joinRequest.ReviewedByUserId = reviewer.Id;
        joinRequest.ReviewedAt = now;

        if (joinRequest.Status == FamilyJoinRequestStatus.Rejected)
        {
            _dbContext.Notifications.Add(new Notification
            {
                FamilyId = familyId,
                RecipientUserId = joinRequest.RequesterUserId,
                ActorUserId = reviewer.Id,
                Type = NotificationType.FamilyJoinRequestRejected,
                Title = "Family join request updated",
                Body = "Your request was not approved. You can request again with a new active code.",
                RelatedJoinRequestId = joinRequest.Id
            });
        }

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictAppException("This join request was reviewed at the same time. Refresh and try again.");
        }

        await Send.OkAsync(new Response(FamilyJoinRequestResponseProjection.FromRequest(joinRequest, now)), ct);
    }
}
