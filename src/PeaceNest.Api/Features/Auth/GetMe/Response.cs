using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Auth.GetMe;

public sealed record Response(
    CurrentUserResponse User,
    IReadOnlyCollection<FamilyMembershipResponse> FamilyMemberships);

public sealed record CurrentUserResponse(
    Guid Id,
    Guid SupabaseUserId,
    string Email,
    string DisplayName,
    string? CountryCode,
    DateTimeOffset? OnboardingCompletedAt,
    string? AvatarUrl,
    string? Timezone,
    DateTimeOffset? LastSeenAt);

public sealed record FamilyMembershipResponse(
    Guid FamilyId,
    string FamilyName,
    string PreferredCurrency,
    FamilyMemberRole Role,
    FamilyMemberStatus Status);
