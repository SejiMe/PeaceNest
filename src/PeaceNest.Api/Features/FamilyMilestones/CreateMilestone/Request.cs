namespace PeaceNest.Api.Features.FamilyMilestones.CreateMilestone;

public sealed record Request(
    string Title,
    string? Description,
    int? PriorityRank,
    int ProgressPercent,
    DateOnly? TargetDate,
    string? MilestoneType,
    string? CelebrationNotes,
    string? ReflectionPrompt,
    bool IncludeInRecap,
    IReadOnlyCollection<CreateMilestoneStepRequest> Steps);

public sealed record CreateMilestoneStepRequest(
    string Title,
    string? Description,
    int? SortOrder);
