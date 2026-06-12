using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;
using PeaceNest.Api.Common.Errors;
using PeaceNest.Api.Common.RateLimiting;

namespace PeaceNest.Api.Features.FamilyMilestones.UpdateMilestoneStepCompletion;

public sealed class Endpoint : Endpoint<Request, Response>
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
        Put("/families/{familyId:guid}/milestones/{milestoneId:guid}/steps/{stepId:guid}/completion");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(builder => builder.RequireRateLimiting(RateLimitPolicyNames.Write));
        Description(builder => builder.WithTags("Family Milestones"));
        Summary(summary =>
        {
            summary.Summary = "Update milestone checklist progress.";
            summary.Description = "Marks one milestone checklist step complete or still growing.";
            summary.Responses[200] = "The milestone checklist step was updated.";
            summary.Responses[403] = "The authenticated family member cannot update this milestone step.";
            summary.Responses[404] = "The milestone or checklist step was not found.";
        });
    }

    public override async Task HandleAsync(Request request, CancellationToken ct)
    {
        var familyId = Route<Guid>("familyId");
        var milestoneId = Route<Guid>("milestoneId");
        var stepId = Route<Guid>("stepId");
        var authenticatedUser = AuthenticatedSupabaseUserFactory.FromClaimsPrincipal(User);
        var user = await _currentUserService.GetOrCreateUserAsync(authenticatedUser, ct);
        await _familyMembershipAuthorizer.RequireActiveMembershipAsync(
            familyId,
            user.Id,
            FamilyRolePermissions.CanUpdateMilestoneSteps,
            "You do not have permission to update milestone checklist progress.",
            ct);

        var milestone = await _dbContext.FamilyPlans
            .Include(plan => plan.MilestoneDetails)
            .Include(plan => plan.GoalSteps)
            .SingleOrDefaultAsync(
                candidate => candidate.Id == milestoneId &&
                    candidate.FamilyId == familyId &&
                    candidate.PlanType == PlanType.Milestone,
                ct);

        if (milestone is null)
        {
            throw new NotFoundAppException("Family milestone was not found.");
        }

        var step = milestone.GoalSteps.SingleOrDefault(candidate => candidate.Id == stepId && candidate.DeletedAt is null);

        if (step is null)
        {
            throw new NotFoundAppException("Milestone checklist step was not found.");
        }

        var now = _timeProvider.GetUtcNow();
        step.IsCompleted = request.IsCompleted;
        step.CompletedByUserId = request.IsCompleted ? user.Id : null;
        step.CompletedAt = request.IsCompleted ? now : null;

        var visibleSteps = milestone.GoalSteps.Where(candidate => candidate.DeletedAt is null).ToArray();
        var completedSteps = visibleSteps.Count(candidate => candidate.IsCompleted);
        milestone.ProgressPercent = visibleSteps.Length == 0
            ? milestone.ProgressPercent
            : (int)Math.Round((decimal)completedSteps / visibleSteps.Length * 100m);

        await _dbContext.SaveChangesAsync(ct);

        await Send.OkAsync(new Response(MilestoneResponseProjection.FromPlan(milestone)), ct);
    }
}
