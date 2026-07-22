using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.WantsAndNeeds;

public static class WantOrNeedResponseProjection
{
    public static WantOrNeedResponse FromPlan(FamilyPlan plan)
    {
        var details = plan.WantNeedDetails!;

        return new WantOrNeedResponse(
            plan.Id,
            plan.FamilyId,
            plan.CreatedByUserId,
            details.Kind,
            plan.Title,
            plan.Description,
            plan.Status,
            plan.PriorityRank,
            plan.PriorityScore,
            plan.ProgressPercent,
            details.EstimatedCostAmount,
            details.EstimatedCostCurrency,
            details.UrgencyLevel,
            details.ImportanceLevel,
            details.EmotionalValueLevel,
            details.DesiredByDate,
            plan.TargetDate,
            plan.Version,
            plan.CreatedAt,
            plan.UpdatedAt);
    }
}
