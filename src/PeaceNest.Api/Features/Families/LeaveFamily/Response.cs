namespace PeaceNest.Api.Features.Families.LeaveFamily;

public sealed record Response(
    Guid FamilyId,
    bool RecoveryAvailable,
    string? RecoveryCode,
    DateTimeOffset? RecoveryExpiresAt);
