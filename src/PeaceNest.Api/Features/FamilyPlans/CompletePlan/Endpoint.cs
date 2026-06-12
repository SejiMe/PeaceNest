using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyPlans.CompletePlan;

public sealed class Endpoint : EndpointWithoutRequest<Response>
{
    private readonly CurrentUserService _currentUserService;
    private readonly FamilyMembershipAuthorizer _familyMembershipAuthorizer;
    private readonly PeaceNestDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public Endpoint(
        CurrentUserService currentUserService,
        FamilyMembershipAuthorizer familyMembershipAuthorizer,
        PeaceNestDbContext dbContext,
        TimeProvider timeProvider)
    {
        _currentUserService = currentUserService;
        _familyMembershipAuthorizer = familyMembershipAuthorizer;
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public override void Configure()
    {
        Put("/families/{familyId:guid}/plans/{planId:guid}/complete");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Plans"));
        Summary(summary =>
        {
            summary.Summary = "Complete a family plan.";
            summary.Description = "Marks a family plan complete and sets manual progress to 100%.";
            summary.Responses[200] = "The family plan was completed.";
            summary.Responses[403] = "The authenticated family member cannot complete this family plan.";
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
            FamilyRolePermissions.CanUpdateFamilyPlans,
            "You do not have permission to complete this family plan.",
            ct);

        var plan = await _dbContext.FamilyPlans
            .SingleOrDefaultAsync(
                candidate => candidate.Id == planId && candidate.FamilyId == familyId,
                ct);

        if (plan is null)
        {
            throw new NotFoundAppException("Family plan was not found.");
        }

        var now = _timeProvider.GetUtcNow();
        plan.Status = PlanStatus.Completed;
        plan.ProgressPercent = 100;
        plan.CompletedAt ??= now;
        plan.ArchivedAt = null;

        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new Response(PlanActionResponseProjection.FromPlan(plan)), ct);
    }
}
