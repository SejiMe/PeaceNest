using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Notifications.ListNotifications;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get("/families/{familyId:guid}/notifications");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Notifications"));
        Summary(summary =>
        {
            summary.Summary = "List notifications.";
            summary.Description = "Lists the authenticated family member's notifications for one family workspace.";
            summary.Responses[200] = "The notifications were returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The family workspace was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to view notifications for this family workspace.",
            ct);

        var notifications = await _dbContext.Notifications
            .AsNoTracking()
            .Include(notification => notification.ActorUser)
            .Where(notification => notification.FamilyId == familyId && notification.RecipientUserId == user.Id)
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
