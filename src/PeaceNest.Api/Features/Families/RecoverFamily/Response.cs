using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Features.Families.RecoverFamily;

public sealed record Response(
    Guid FamilyId,
    string FamilyName,
    string PreferredCurrency,
    FamilyMemberRole Role,
    DateTimeOffset RecoveredAt);
