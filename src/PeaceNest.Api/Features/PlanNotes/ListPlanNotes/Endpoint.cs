using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.PlanNotes.ListPlanNotes;

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
        Get("/families/{familyId:guid}/plans/{planId:guid}/notes");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Plan Notes"));
        Summary(summary =>
        {
            summary.Summary = "List plan notes.";
            summary.Description = "Lists simple plan-level notes for an authorized family member.";
            summary.Responses[200] = "The plan notes were returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The family plan was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("planId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to view notes for this family plan.",
            ct);

        var planExists = await _dbContext.FamilyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == planId && plan.FamilyId == familyId, ct);

        if (!planExists)
        {
            throw new NotFoundAppException("Family plan was not found.");
        }

        var notes = await _dbContext.Comments
            .AsNoTracking()
            .Include(comment => comment.AuthorUser)
            .Where(comment => comment.PlanId == planId && comment.ParentCommentId == null)
            .OrderBy(comment => comment.CreatedAt)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response(notes.Select(PlanNoteResponseProjection.FromComment).ToArray()),
            ct);
    }
}
