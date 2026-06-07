using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Families.ListFamilies;

public sealed record Response(IReadOnlyCollection<FamilyWorkspaceResponse> Families);

public sealed record FamilyWorkspaceResponse(
    Guid Id,
    string Name,
    string? Description,
    FamilyMemberRole CurrentUserRole,
    int MemberCount,
    DateTimeOffset CreatedAt);
