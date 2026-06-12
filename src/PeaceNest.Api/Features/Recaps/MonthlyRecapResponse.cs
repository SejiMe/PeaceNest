using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Recaps;

public sealed record MonthlyRecapResponse(
    Guid Id,
    Guid FamilyId,
    RecapPeriodType PeriodType,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    string Title,
    string? Summary,
    MonthlyRecapStatsResponse Stats,
    Guid GeneratedByUserId,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<RecapItemResponse> Items);
