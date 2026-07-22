using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Features.Notifications.MarkNotificationRead;

namespace PeaceNest.Api.Features.Notifications.MarkMyNotificationRead;

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
        Put("/notifications/{notificationId:guid}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Notifications"));
        Summary(summary => summary.Summary = "Mark my notification as read.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var notificationId = Route<Guid>("notificationId");
        var authenticated = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticated, ct);
        var notification = await _dbContext.Notifications
            .Include(candidate => candidate.ActorUser)
            .SingleOrDefaultAsync(candidate =>
                candidate.Id == notificationId && candidate.RecipientUserId == user.Id,
                ct)
            ?? throw new NotFoundAppException("Notification was not found.");

        notification.ReadAt ??= _timeProvider.GetUtcNow();
        await _dbContext.SaveChangesAsync(ct);
        await Send.OkAsync(new Response(NotificationResponseProjection.FromNotification(notification)), ct);
    }
}
