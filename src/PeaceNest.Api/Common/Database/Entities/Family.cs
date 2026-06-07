namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Family : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<FamilyMember> Members { get; } = new List<FamilyMember>();

    public ICollection<FamilyInvitation> Invitations { get; } = new List<FamilyInvitation>();
}
