using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyJoinRequests.ApproveJoinRequest;

public sealed class Endpoint : Endpoint<Request, Response>
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
        Post("/families/{familyId:guid}/join-requests/{requestId:guid}/approve");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Join Requests"));
        Summary(summary => summary.Summary = "Approve a family join request with a role.");
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var requestId = Route<Guid>("requestId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var reviewer = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        var reviewerMembership = await _authorizer.RequireActiveMembershipAsync(
            familyId,
            reviewer.Id,
            FamilyRolePermissions.CanReviewFamilyJoinRequests,
            "Only family owners and parent/admins can approve join requests.",
            ct);

        if (!Enum.IsDefined(request.Role) ||
            !FamilyRolePermissions.CanAssignRoleFromJoinRequest(reviewerMembership.Role, request.Role))
        {
            throw new AuthorizationAppException(
                request.Role == FamilyMemberRole.ParentAdmin
                    ? "Only the family owner can approve a parent/admin role."
                    : "This family role cannot be assigned from a join request.");
        }

        var now = _timeProvider.GetUtcNow();
        var joinRequest = await _dbContext.FamilyJoinRequests
            .Include(candidate => candidate.Family)
            .Include(candidate => candidate.RequesterUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == requestId && candidate.FamilyId == familyId, ct)
            ?? throw new NotFoundAppException("Family join request was not found.");

        if (joinRequest.Status == FamilyJoinRequestStatus.Approved)
        {
            await Send.OkAsync(new Response(FamilyJoinRequestResponseProjection.FromRequest(joinRequest, now)), ct);
            return;
        }

        if (joinRequest.Status != FamilyJoinRequestStatus.Pending)
        {
            throw new DomainRuleAppException("Only a pending family join request can be approved.");
        }

        if (joinRequest.ExpiresAt <= now)
        {
            joinRequest.Status = FamilyJoinRequestStatus.Expired;
            await _dbContext.SaveChangesAsync(ct);
            throw new DomainRuleAppException("This family join request has expired.");
        }

        var membership = await _dbContext.FamilyMembers
            .SingleOrDefaultAsync(member =>
                member.FamilyId == familyId && member.UserId == joinRequest.RequesterUserId,
                ct);
        if (membership is { Status: FamilyMemberStatus.Active })
        {
            throw new ConflictAppException("This user already belongs to the family workspace.");
        }

        membership ??= new FamilyMember
        {
            FamilyId = familyId,
            UserId = joinRequest.RequesterUserId
        };
        membership.Role = request.Role;
        membership.Status = FamilyMemberStatus.Active;
        membership.JoinedAt = now;
        membership.RemovedAt = null;
        if (membership.Id == Guid.Empty)
        {
            _dbContext.FamilyMembers.Add(membership);
        }

        joinRequest.Status = FamilyJoinRequestStatus.Approved;
        joinRequest.ApprovedRole = request.Role;
        joinRequest.ReviewedByUserId = reviewer.Id;
        joinRequest.ReviewedAt = now;
        _dbContext.Notifications.Add(new Notification
        {
            FamilyId = familyId,
            RecipientUserId = joinRequest.RequesterUserId,
            ActorUserId = reviewer.Id,
            Type = NotificationType.FamilyJoinRequestApproved,
            Title = "Family join request approved",
            Body = "You now have access to the family workspace.",
            RelatedJoinRequestId = joinRequest.Id
        });

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
