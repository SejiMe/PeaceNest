using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Features.Notifications.ListNotifications;

namespace PeaceNest.Api.Features.Notifications.ListMyNotifications;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly PeaceNestDbContext _dbContext;

    public Endpoint(CurrentUserService currentUserService, PeaceNestDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/notifications");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Notifications"));
        Summary(summary =>
        {
            summary.Summary = "List my notifications.";
            summary.Description = "Lists notifications owned by the authenticated recipient across family workspaces.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Include(notification => notification.ActorUser)
            .Where(notification => notification.RecipientUserId == user.Id)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response(
                notifications.Select(NotificationResponseProjection.FromNotification).ToArray(),
                notifications.Count(notification => notification.ReadAt is null)),
            ct);
    }
}
