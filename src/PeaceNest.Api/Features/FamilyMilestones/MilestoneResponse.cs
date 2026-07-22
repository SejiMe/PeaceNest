using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyMilestones;

public sealed record MilestoneResponse(
    Guid Id,
    Guid FamilyId,
    Guid CreatedByUserId,
    string Title,
    string? Description,
    PlanStatus Status,
    int? PriorityRank,
    decimal PriorityScore,
    int ProgressPercent,
    DateOnly? TargetDate,
    string? MilestoneType,
    string? CelebrationNotes,
    string? ReflectionPrompt,
    bool IncludeInRecap,
    IReadOnlyCollection<MilestoneStepResponse> Steps,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record MilestoneStepResponse(
    Guid Id,
    string Title,
    string? Description,
    int SortOrder,
    bool IsCompleted,
    Guid? CompletedByUserId,
    DateTimeOffset? CompletedAt);
