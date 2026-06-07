using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;

namespace PeaceNest.Api.Features.WantsAndNeeds.GetWantOrNeed;

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
        Get("/families/{familyId:guid}/wants-needs/{wantOrNeedId:guid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Description(builder => builder.WithTags("Wants & Needs"));
        Summary(summary =>
        {
            summary.Summary = "Get a want or need.";
            summary.Description = "Returns one Wants & Needs family plan for an authorized family member.";
            summary.Responses[200] = "The want or need was returned.";
            summary.Responses[403] = "The authenticated user is not a member of this family workspace.";
            summary.Responses[404] = "The want or need was not found.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var wantOrNeedId = Route<Guid>("wantOrNeedId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanViewFamily,
            "You do not have permission to view Wants & Needs for this family workspace.",
            ct);

        var plan = await _dbContext.FamilyPlans
            .AsNoTracking()
            .Include(plan => plan.WantNeedDetails)
            .SingleOrDefaultAsync(
                plan => plan.Id == wantOrNeedId &&
                    plan.FamilyId == familyId &&
                    plan.PlanType == PlanType.WantNeed,
                ct);

        if (plan is null)
        {
            throw new NotFoundAppException("Want or need was not found.");
        }

        await Send.OkAsync(new Response(WantOrNeedResponseProjection.FromPlan(plan)), ct);
    }
}
