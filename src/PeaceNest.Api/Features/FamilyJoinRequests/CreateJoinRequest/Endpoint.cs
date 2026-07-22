using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.JoinCodes;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyJoinRequests.CreateJoinRequest;

public sealed class Endpoint : Endpoint<Request, Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly FamilyJoinCodeService _codeService;
    private readonly JoinCodePolicy _policy;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        PeaceNestDbContext dbContext,
        FamilyJoinCodeService codeService,
        JoinCodePolicy policy,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _codeService = codeService;
        _policy = policy;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/family-join-requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Invite));
        Description(builder => builder.WithTags("Family Join Requests"));
        Summary(summary =>
        {
            summary.Summary = "Request to join a family with a temporary code.";
            summary.Description = "Creates a pending request without granting family access.";
            summary.Responses[201] = "The join request was created.";
            summary.Responses[200] = "The existing pending request was returned.";
            summary.Responses[409] = "The authenticated user already belongs to the family.";
            summary.Responses[422] = "The join code is invalid, expired, revoked, or at capacity.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || !_codeService.IsValid(request.Code))
        {
            throw new ValidationAppException(
                "Family join request is invalid.",
                [new ValidationFailure("code", "Enter a valid 10-character family join code.")]);
        }

        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        CurrentUserService.RequireCompletedProfile(user);
        var now = _timeProvider.GetUtcNow();
        var codeHash = _codeService.Hash(request.Code);
        var code = await _dbContext.FamilyJoinCodes
            .Include(candidate => candidate.Family)
            .SingleOrDefaultAsync(candidate => candidate.CodeHash == codeHash, ct);

        if (code is null)
        {
            throw new DomainRuleAppException("This family join code is invalid or no longer available.");
        }

        var activeMembership = await _dbContext.FamilyMembers
            .AnyAsync(member =>
                member.FamilyId == code.FamilyId &&
                member.UserId == user.Id &&
                member.Status == FamilyMemberStatus.Active,
                ct);
        if (activeMembership)
        {
            throw new ConflictAppException("You already belong to this family workspace.");
        }

        var latestRequest = await _dbContext.FamilyJoinRequests
            .Include(joinRequest => joinRequest.Family)
            .Include(joinRequest => joinRequest.RequesterUser)
            .Where(joinRequest => joinRequest.FamilyId == code.FamilyId && joinRequest.RequesterUserId == user.Id)
            .OrderByDescending(joinRequest => joinRequest.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (latestRequest is { Status: FamilyJoinRequestStatus.Pending } && latestRequest.ExpiresAt > now)
        {
            await Send.OkAsync(
                new Response(FamilyJoinRequestResponseProjection.FromRequest(latestRequest, now), true),
                ct);
            return;
        }

        if (latestRequest is { Status: FamilyJoinRequestStatus.Pending })
        {
            latestRequest.Status = FamilyJoinRequestStatus.Expired;
        }

        if (code.Status != FamilyJoinCodeStatus.Active || _policy.IsExpired(code.ExpiresAt, now))
        {
            if (code.Status == FamilyJoinCodeStatus.Active)
            {
                code.Status = FamilyJoinCodeStatus.Expired;
                await _dbContext.SaveChangesAsync(ct);
            }

            throw new DomainRuleAppException("This family join code is invalid or no longer active.");
        }

        if (!_policy.HasCapacity(code))
        {
            code.Status = FamilyJoinCodeStatus.CapacityReached;
            await _dbContext.SaveChangesAsync(ct);
            throw new DomainRuleAppException("This family join code has reached its request limit.");
        }

        var joinRequest = new FamilyJoinRequest
        {
            FamilyId = code.FamilyId,
            Family = code.Family,
            JoinCodeId = code.Id,
            JoinCode = code,
            RequesterUserId = user.Id,
            RequesterUser = user,
            Status = FamilyJoinRequestStatus.Pending,
            ExpiresAt = _policy.GetRequestExpiry(now)
        };
        code.RequestCount++;
        if (!_policy.HasCapacity(code))
        {
            code.Status = FamilyJoinCodeStatus.CapacityReached;
        }

        _dbContext.FamilyJoinRequests.Add(joinRequest);
        await AddReviewerNotificationsAsync(code.FamilyId, user, joinRequest, ct);

        try
        {
            await _dbContext.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictAppException("The join code was used at the same time. Please try again.");
        }

        await Send.CreatedAtAsync(
            nameof(ListMyJoinRequests.Endpoint),
            routeValues: null,
            new Response(FamilyJoinRequestResponseProjection.FromRequest(joinRequest, now), false),
            cancellation: ct);
    }

    private async Task AddReviewerNotificationsAsync(
        Guid familyId,
        User requester,
        FamilyJoinRequest joinRequest,
        CancellationToken cancellationToken)
    {
        var reviewerIds = await _dbContext.FamilyMembers
            .Where(member =>
                member.FamilyId == familyId &&
                member.Status == FamilyMemberStatus.Active &&
                (member.Role == FamilyMemberRole.Owner || member.Role == FamilyMemberRole.ParentAdmin))
            .Select(member => member.UserId)
            .ToListAsync(cancellationToken);

        foreach (var reviewerId in reviewerIds)
        {
            _dbContext.Notifications.Add(new Notification
            {
                FamilyId = familyId,
                RecipientUserId = reviewerId,
                ActorUserId = requester.Id,
                Type = NotificationType.FamilyJoinRequestCreated,
                Title = "New family join request",
                Body = $"{requester.DisplayName} asked to join your family workspace.",
                RelatedJoinRequestId = joinRequest.Id
            });
        }
    }
}
