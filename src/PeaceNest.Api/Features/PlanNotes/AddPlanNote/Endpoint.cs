using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;
using PeaceNest.Api.Features.PlanNotes.ListPlanNotes;

namespace PeaceNest.Api.Features.PlanNotes.AddPlanNote;

public sealed class Endpoint : Endpoint<Request, Response>
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
        Post("/families/{familyId:guid}/plans/{planId:guid}/notes");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Plan Notes"));
        Summary(summary =>
        {
            summary.Summary = "Add a plan note.";
            summary.Description = "Adds a simple plan-level note to a family plan.";
            summary.Responses[201] = "The plan note was added.";
            summary.Responses[400] = "The note request was invalid.";
            summary.Responses[403] = "The authenticated family member cannot add notes.";
            summary.Responses[404] = "The family plan was not found.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        ValidateRequest(request);

        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("planId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanAddPlanNotes,
            "You do not have permission to add notes for this family plan.",
            ct);

        var planExists = await _dbContext.FamilyPlans
            .AsNoTracking()
            .AnyAsync(plan => plan.Id == planId && plan.FamilyId == familyId, ct);

        if (!planExists)
        {
            throw new NotFoundAppException("Family plan was not found.");
        }

        var note = new Comment
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            AuthorUserId = user.Id,
            ParentCommentId = null,
            Body = request.Body.Trim()
        };

        _dbContext.Comments.Add(note);
        await _dbContext.SaveChangesAsync(ct);

        note.AuthorUser = user;

        await Send.CreatedAtAsync(
            nameof(ListPlanNotes.Endpoint),
            new { familyId, planId },
            new Response(PlanNoteResponseProjection.FromComment(note)),
            cancellation: ct);
    }

    private static void ValidateRequest(Request request)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            failures.Add(new ValidationFailure("body", "A note is required."));
        }
        else if (request.Body.Trim().Length > 4000)
        {
            failures.Add(new ValidationFailure("body", "Note must be 4000 characters or fewer."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationAppException("Plan note request is invalid.", failures);
        }
    }
}
