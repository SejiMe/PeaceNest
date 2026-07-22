namespace PeaceNest.Api.Features.Auth.CompleteProfile;

public sealed record Response(
    Guid Id,
    string Email,
    string DisplayName,
    string CountryCode,
    DateTimeOffset OnboardingCompletedAt);
