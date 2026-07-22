using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyJoinCodes.GenerateJoinCode;

public sealed record Response(
    Guid Id,
    string Code,
    FamilyJoinCodeStatus Status,
    int RequestCount,
    int MaxRequests,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
