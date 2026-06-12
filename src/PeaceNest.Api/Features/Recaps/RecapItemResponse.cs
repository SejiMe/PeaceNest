namespace PeaceNest.Api.Features.Recaps;

public sealed record RecapItemResponse(
    Guid Id,
    Guid? PlanId,
    Guid? MemoryId,
    string ItemType,
    string Title,
    string? Description,
    int SortOrder,
    DateTimeOffset CreatedAt);
