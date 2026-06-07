using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyMilestones.ListMilestones;

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
        Get("/families/{familyId:guid}/milestones");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Family Milestones"));
        Summary(summary =>
        {
            summary.Summary = "List family milestones.";
            summary.Description = "Lists active milestone family plans for an authorized family member.";
            summary.Responses[200] = "The family milestones were returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
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
            "You do not have permission to view Family Milestones for this family workspace.",
            ct);

        var plans = await _dbContext.FamilyPlans
            .AsNoTracking()
            .Include(plan => plan.MilestoneDetails)
            .Include(plan => plan.GoalSteps)
            .Where(plan => plan.FamilyId == familyId &&
                plan.PlanType == PlanType.Milestone &&
                plan.Status == PlanStatus.Active)
            .OrderBy(plan => plan.PriorityRank == null)
            .ThenBy(plan => plan.PriorityRank)
            .ThenBy(plan => plan.TargetDate == null)
            .ThenBy(plan => plan.TargetDate)
            .ThenBy(plan => plan.Title)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response(plans.Select(MilestoneResponseProjection.FromPlan).ToArray()),
            ct);
    }
}
