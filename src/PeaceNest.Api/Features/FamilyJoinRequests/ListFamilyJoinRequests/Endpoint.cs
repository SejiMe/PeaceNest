using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyJoinRequests.ListFamilyJoinRequests;

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
        Get("/families/{familyId:guid}/join-requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Family Join Requests"));
        Summary(summary => summary.Summary = "List pending family join requests.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        await _authorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanReviewFamilyJoinRequests,
            "Only family owners and parent/admins can review join requests.",
            ct);

        var now = _timeProvider.GetUtcNow();
        var requests = await _dbContext.FamilyJoinRequests
            .Include(request => request.Family)
            .Include(request => request.RequesterUser)
            .Where(request => request.FamilyId == familyId && request.Status == FamilyJoinRequestStatus.Pending)
            .OrderBy(request => request.CreatedAt)
            .ToListAsync(ct);
        var expired = requests.Where(request => request.ExpiresAt <= now).ToArray();
        foreach (var request in expired)
        {
            request.Status = FamilyJoinRequestStatus.Expired;
        }

        if (expired.Length > 0)
        {
            await _dbContext.SaveChangesAsync(ct);
        }

        await Send.OkAsync(
            new Response(requests
                .Where(request => request.Status == FamilyJoinRequestStatus.Pending)
                .Select(request => FamilyJoinRequestResponseProjection.FromRequest(request, now))
                .ToArray()),
            ct);
    }
}
