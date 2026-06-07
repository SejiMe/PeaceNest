using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.WantsAndNeeds.ListWantsAndNeeds;

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
        Get("/families/{familyId:guid}/wants-needs");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Wants & Needs"));
        Summary(summary =>
        {
            summary.Summary = "List wants and needs.";
            summary.Description = "Lists active Wants & Needs family plans for an authorized family member.";
            summary.Responses[200] = "The wants and needs were returned.";
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
            "You do not have permission to view Wants & Needs for this family workspace.",
            ct);

        var plans = await _dbContext.FamilyPlans
            .AsNoTracking()
            .Include(plan => plan.WantNeedDetails)
            .Where(plan => plan.FamilyId == familyId &&
                plan.PlanType == PlanType.WantNeed &&
                plan.Status == PlanStatus.Active)
            .OrderBy(plan => plan.PriorityRank == null)
            .ThenBy(plan => plan.PriorityRank)
            .ThenByDescending(plan => plan.PriorityScore)
            .ThenBy(plan => plan.Title)
            .ToListAsync(ct);

        await Send.OkAsync(
            new Response(plans.Select(WantOrNeedResponseProjection.FromPlan).ToArray()),
            ct);
    }
}
