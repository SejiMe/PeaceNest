using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.FamilyMilestones.GetMilestone;

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
        Get("/families/{familyId:guid}/milestones/{milestoneId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Family Milestones"));
        Summary(summary =>
        {
            summary.Summary = "Get a family milestone.";
            summary.Description = "Returns one milestone family plan for an authorized family member.";
            summary.Responses[200] = "The family milestone was returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The family milestone was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var milestoneId = Route<Guid>("milestoneId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to view Family Milestones for this family workspace.",
            ct);

        var plan = await _dbContext.FamilyPlans
            .AsNoTracking()
            .Include(plan => plan.MilestoneDetails)
            .Include(plan => plan.GoalSteps)
            .SingleOrDefaultAsync(
                plan => plan.Id == milestoneId &&
                    plan.FamilyId == familyId &&
                    plan.PlanType == PlanType.Milestone,
                ct);

        if (plan is null)
        {
            throw new NotFoundAppException("Family milestone was not found.");
        }

        await Send.OkAsync(new Response(MilestoneResponseProjection.FromPlan(plan)), ct);
    }
}
