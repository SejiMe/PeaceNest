using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.WantsAndNeeds;

public sealed record WantOrNeedResponse(
    Guid Id,
    Guid FamilyId,
    Guid CreatedByUserId,
    WantNeedKind Kind,
    string Title,
    string? Description,
    PlanStatus Status,
    int? PriorityRank,
    decimal PriorityScore,
    int ProgressPercent,
    decimal? EstimatedCostAmount,
    string? EstimatedCostCurrency,
    ScoreLevel UrgencyLevel,
    ScoreLevel ImportanceLevel,
    ScoreLevel EmotionalValueLevel,
    DateOnly? DesiredByDate,
    DateOnly? TargetDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
