using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyMilestones;

public static class MilestoneResponseProjection
{
    public static MilestoneResponse FromPlan(FamilyPlan plan)
    {
        var details = plan.MilestoneDetails!;
        var steps = plan.GoalSteps
            .Where(step => step.DeletedAt is null)
            .OrderBy(step => step.SortOrder)
            .ThenBy(step => step.Title)
            .Select(step => new MilestoneStepResponse(
                step.Id,
                step.Title,
                step.Description,
                step.SortOrder,
                step.IsCompleted,
                step.CompletedByUserId,
                step.CompletedAt))
            .ToArray();

        return new MilestoneResponse(
            plan.Id,
            plan.FamilyId,
            plan.CreatedByUserId,
            plan.Title,
            plan.Description,
            plan.Status,
            plan.PriorityRank,
            plan.PriorityScore,
            plan.ProgressPercent,
            plan.TargetDate,
            details.MilestoneType,
            details.CelebrationNotes,
            details.ReflectionPrompt,
            details.IncludeInRecap,
            steps,
            plan.CreatedAt,
            plan.UpdatedAt);
    }
}
