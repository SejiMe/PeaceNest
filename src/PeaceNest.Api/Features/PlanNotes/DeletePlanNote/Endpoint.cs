using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.PlanNotes.DeletePlanNote;

public sealed class Endpoint : EndpointWithoutRequest
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
        Delete("/families/{familyId:guid}/plans/{planId:guid}/notes/{noteId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Plan Notes"));
        Summary(summary =>
        {
            summary.Summary = "Delete a plan note.";
            summary.Description = "Soft-deletes a plan-level note by its author or a family note moderator.";
            summary.Responses[204] = "The plan note was deleted.";
            summary.Responses[403] = "The authenticated family member cannot delete this note.";
            summary.Responses[404] = "The family plan or note was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("planId");
        var noteId = Route<Guid>("noteId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        var membership = await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to delete notes for this family plan.",
            ct);

        var note = await _dbContext.Comments
            .Include(comment => comment.Plan)
            .SingleOrDefaultAsync(
                comment => comment.Id == noteId &&
                    comment.PlanId == planId &&
                    comment.Plan.FamilyId == familyId &&
                    comment.ParentCommentId == null,
                ct);

        if (note is null)
        {
            throw new NotFoundAppException("Plan note was not found.");
        }

        var isAuthor = note.AuthorUserId == user.Id;
        var canModerate = FamilyRolePermissions.CanModeratePlanNotes(membership.Role);

        if (!isAuthor && !canModerate)
        {
            throw new AuthorizationAppException("You do not have permission to delete this plan note.");
        }

        _dbContext.Comments.Remove(note);
        await _dbContext.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
