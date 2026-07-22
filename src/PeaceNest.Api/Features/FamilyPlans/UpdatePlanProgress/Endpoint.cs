using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyPlans.UpdatePlanProgress;

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
        Put("/families/{familyId:guid}/plans/{planId:guid}/progress");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Plans"));
        Summary(summary =>
        {
            summary.Summary = "Update manual plan progress.";
            summary.Description = "Updates lightweight manual progress for an authorized family plan.";
            summary.Responses[200] = "The plan progress was updated.";
            summary.Responses[400] = "The progress request was invalid.";
            summary.Responses[403] = "The authenticated family member cannot update this family plan.";
            summary.Responses[404] = "The family plan was not found.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        if (request.ProgressPercent is < 0 or > 100)
        {
            throw new ValidationAppException(
                "Plan progress request is invalid.",
                [new ValidationFailure("progressPercent", "Progress must be between 0 and 100.")]);
        }

        var familyId = Route<Guid>("familyId");
        var planId = Route<Guid>("planId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanUpdateFamilyPlans,
            "You do not have permission to update this family plan.",
            ct);

        var plan = await _dbContext.FamilyPlans
            .SingleOrDefaultAsync(
                candidate => candidate.Id == planId && candidate.FamilyId == familyId,
                ct);

        if (plan is null)
        {
            throw new NotFoundAppException("Family plan was not found.");
        }

        if (plan.Status != PlanStatus.Active)
        {
            throw new DomainRuleAppException("Completed and archived family plans are read-only.");
        }

        plan.ProgressPercent = request.ProgressPercent;
        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new Response(PlanActionResponseProjection.FromPlan(plan)), ct);
    }
}
