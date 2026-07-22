using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class FamilyJoinCode : IUsesVersion7Guid, IAuditableEntity, IConcurrencyTrackedEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public string CodeHash { get; set; } = string.Empty;

    public FamilyJoinCodeStatus Status { get; set; }

    public int RequestCount { get; set; }

    public int MaxRequests { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public Guid? RevokedByUserId { get; set; }

    public User? RevokedByUser { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int Version { get; set; }

    public ICollection<FamilyJoinRequest> Requests { get; } = new List<FamilyJoinRequest>();
}
