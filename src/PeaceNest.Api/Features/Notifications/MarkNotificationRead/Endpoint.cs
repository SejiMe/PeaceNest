using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Notifications.MarkNotificationRead;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Put("/families/{familyId:guid}/notifications/{notificationId:guid}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Notifications"));
        Summary(summary =>
        {
            summary.Summary = "Mark a notification as read.";
            summary.Description = "Marks one notification as read for the authenticated recipient.";
            summary.Responses[200] = "The notification was marked as read.";
            summary.Responses[403] = "The authenticated user cannot update this notification.";
            summary.Responses[404] = "The notification was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var notificationId = Route<Guid>("notificationId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to update notifications for this family workspace.",
            ct);

        var notification = await _dbContext.Notifications
            .Include(candidate => candidate.ActorUser)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == notificationId &&
                    candidate.FamilyId == familyId &&
                    candidate.RecipientUserId == user.Id,
                ct);

        if (notification is null)
        {
            throw new NotFoundAppException("Notification was not found.");
        }

        notification.ReadAt ??= _timeProvider.GetUtcNow();
        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(
            new Response(NotificationResponseProjection.FromNotification(notification)),
            ct);
    }
}
