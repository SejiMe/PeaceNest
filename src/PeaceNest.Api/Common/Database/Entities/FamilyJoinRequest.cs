using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class FamilyJoinRequest : IUsesVersion7Guid, IAuditableEntity, IConcurrencyTrackedEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid JoinCodeId { get; set; }

    public FamilyJoinCode JoinCode { get; set; } = null!;

    public Guid RequesterUserId { get; set; }

    public User RequesterUser { get; set; } = null!;

    public FamilyJoinRequestStatus Status { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public User? ReviewedByUser { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public FamilyMemberRole? ApprovedRole { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public int Version { get; set; }

    public ICollection<Notification> Notifications { get; } = new List<Notification>();
}
