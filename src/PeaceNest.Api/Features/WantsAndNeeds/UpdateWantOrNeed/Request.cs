using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.WantsAndNeeds.UpdateWantOrNeed;

public sealed record Request(
    WantNeedKind Kind,
    string Title,
    string? Description,
    int? PriorityRank,
    decimal? EstimatedCostAmount,
    string? EstimatedCostCurrency,
    ScoreLevel UrgencyLevel,
    ScoreLevel ImportanceLevel,
    ScoreLevel EmotionalValueLevel,
    DateOnly? DesiredByDate,
    DateOnly? TargetDate,
    int Version);
