using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.Invitations.GetInvitation;

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
        Get("/families/{familyId:guid}/invitations/{invitationId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Invitations"));
        Summary(summary =>
        {
            summary.Summary = "Get a family invitation.";
            summary.Description = "Returns invitation metadata for family members allowed to invite others.";
            summary.Responses[200] = "The family invitation was returned.";
            summary.Responses[403] = "The authenticated family member cannot view invitations.";
            summary.Responses[404] = "The invitation was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var invitationId = Route<Guid>("invitationId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanInviteFamilyMembers,
            "Only family owners and parent/admins can view family invitations.",
            ct);

        var invitation = await _dbContext.FamilyInvitations
            .AsNoTracking()
            .Where(invitation => invitation.FamilyId == familyId && invitation.Id == invitationId)
            .Select(invitation => new Response(
                invitation.Id,
                invitation.FamilyId,
                invitation.InvitedEmail,
                invitation.InvitedRole,
                invitation.Status,
                invitation.ExpiresAt,
                invitation.CreatedAt))
            .SingleOrDefaultAsync(ct);

        if (invitation is null)
        {
            throw new NotFoundAppException("Family invitation was not found.");
        }

        await Send.OkAsync(invitation, ct);
    }
}
