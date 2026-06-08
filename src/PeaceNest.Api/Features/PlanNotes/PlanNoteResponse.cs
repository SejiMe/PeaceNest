namespace PeaceNest.Api.Features.PlanNotes;

public sealed record PlanNoteResponse(
    Guid Id,
    Guid PlanId,
    Guid AuthorUserId,
    string AuthorDisplayName,
    string Body,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
