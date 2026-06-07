namespace PeaceNest.Api.Common.Database.Entities;

public sealed class FamilyMember : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public FamilyMemberRole Role { get; set; }

    public FamilyMemberStatus Status { get; set; } = FamilyMemberStatus.Active;

    public string? Nickname { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public DateTimeOffset? RemovedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
