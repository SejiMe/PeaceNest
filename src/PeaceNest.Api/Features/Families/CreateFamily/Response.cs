using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Families.CreateFamily;

public sealed record Response(
    Guid Id,
    string Name,
    string? Description,
    string PreferredCurrency,
    FamilyMemberRole CurrentUserRole,
    int MemberCount,
    DateTimeOffset CreatedAt);
