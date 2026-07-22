using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.FamilyJoinCodes.GetJoinCode;

public sealed record Response(
    bool HasActiveCode,
    Guid? Id,
    FamilyJoinCodeStatus? Status,
    int? RequestCount,
    int? MaxRequests,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? ExpiresAt);
