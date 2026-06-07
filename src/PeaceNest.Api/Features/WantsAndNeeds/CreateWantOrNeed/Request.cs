using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.WantsAndNeeds.CreateWantOrNeed;

public sealed record Request(
    WantNeedKind Kind,
    string Title,
    string? Description,
    int? PriorityRank,
    int ProgressPercent,
    decimal? EstimatedCostAmount,
    string? EstimatedCostCurrency,
    ScoreLevel UrgencyLevel,
    ScoreLevel ImportanceLevel,
    ScoreLevel EmotionalValueLevel,
    DateOnly? DesiredByDate,
    DateOnly? TargetDate);
