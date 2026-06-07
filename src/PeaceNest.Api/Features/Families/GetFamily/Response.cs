using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Families.GetFamily;

public sealed record Response(
    Guid Id,
    string Name,
    string? Description,
    FamilyMemberRole CurrentUserRole,
    int MemberCount,
    DateTimeOffset CreatedAt);
