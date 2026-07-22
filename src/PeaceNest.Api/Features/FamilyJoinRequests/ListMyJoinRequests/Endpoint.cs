using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Features.FamilyJoinRequests.ListMyJoinRequests;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Get("/family-join-requests/mine");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Family Join Requests"));
        Summary(summary => summary.Summary = "List my family join requests.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        var now = _timeProvider.GetUtcNow();
        var requests = await _dbContext.FamilyJoinRequests
            .AsNoTracking()
            .Include(request => request.Family)
            .Include(request => request.RequesterUser)
            .Where(request => request.RequesterUserId == user.Id)
            .OrderByDescending(request => request.CreatedAt)
            .Take(25)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response(requests.Select(request =>
                FamilyJoinRequestResponseProjection.FromRequest(request, now)).ToArray()),
            ct);
    }
}
