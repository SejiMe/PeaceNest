using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyPlans;

public sealed record PlanActionResponse(
    Guid PlanId,
    Guid FamilyId,
    PlanStatus Status,
    int ProgressPercent,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset UpdatedAt);

public static class PlanActionResponseProjection
{
    public static PlanActionResponse FromPlan(FamilyPlan plan) =>
        new(
            plan.Id,
            plan.FamilyId,
            plan.Status,
            plan.ProgressPercent,
            plan.CompletedAt,
            plan.ArchivedAt,
            plan.UpdatedAt);
}
