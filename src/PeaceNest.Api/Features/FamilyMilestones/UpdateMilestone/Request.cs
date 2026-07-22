namespace PeaceNest.Api.Features.FamilyMilestones.UpdateMilestone;

public sealed record Request(
    string Title,
    string? Description,
    int? PriorityRank,
    DateOnly? TargetDate,
    string? MilestoneType,
    string? CelebrationNotes,
    string? ReflectionPrompt,
    bool IncludeInRecap,
    IReadOnlyCollection<UpdateMilestoneStepRequest> Steps,
    int Version);

public sealed record UpdateMilestoneStepRequest(
    Guid? Id,
    string Title,
    string? Description,
    int SortOrder);
