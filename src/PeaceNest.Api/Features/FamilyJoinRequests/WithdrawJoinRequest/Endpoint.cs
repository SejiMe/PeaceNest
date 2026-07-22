using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyJoinRequests.WithdrawJoinRequest;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(CurrentUserService currentUserService, PeaceNestDbContext dbContext, TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Post("/family-join-requests/{requestId:guid}/withdraw");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Join Requests"));
        Summary(summary => summary.Summary = "Withdraw my pending family join request.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var requestId = Route<Guid>("requestId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        var now = _timeProvider.GetUtcNow();
        var joinRequest = await _dbContext.FamilyJoinRequests
            .Include(request => request.Family)
            .Include(request => request.RequesterUser)
            .SingleOrDefaultAsync(request => request.Id == requestId && request.RequesterUserId == user.Id, ct)
            ?? throw new NotFoundAppException("Family join request was not found.");

        if (joinRequest.Status == FamilyJoinRequestStatus.Withdrawn)
        {
            await Send.OkAsync(new Response(FamilyJoinRequestResponseProjection.FromRequest(joinRequest, now)), ct);
            return;
        }

        if (joinRequest.Status != FamilyJoinRequestStatus.Pending)
        {
            throw new DomainRuleAppException("Only a pending family join request can be withdrawn.");
        }

        joinRequest.Status = joinRequest.ExpiresAt <= now
            ? FamilyJoinRequestStatus.Expired
            : FamilyJoinRequestStatus.Withdrawn;
        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new Response(FamilyJoinRequestResponseProjection.FromRequest(joinRequest, now)), ct);
    }
}
