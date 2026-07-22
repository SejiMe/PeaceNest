using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class FamilyRecoveryCode : IUsesVersion7Guid, IAuditableEntity, IConcurrencyTrackedEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid CreatorUserId { get; set; }

    public User CreatorUser { get; set; } = null!;

    public string CodeHash { get; set; } = string.Empty;

    public FamilyRecoveryCodeStatus Status { get; set; } = FamilyRecoveryCodeStatus.Active;

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public DateTimeOffset? PurgeClaimedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int Version { get; set; }
}
