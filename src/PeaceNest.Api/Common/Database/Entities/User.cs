namespace PeaceNest.Api.Common.Database.Entities;

public sealed class User : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid SupabaseUserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Timezone { get; set; }

    public DateTimeOffset? LastSeenAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Family> CreatedFamilies { get; } = new List<Family>();

    public ICollection<FamilyMember> FamilyMemberships { get; } = new List<FamilyMember>();

    public ICollection<FamilyInvitation> CreatedInvitations { get; } = new List<FamilyInvitation>();

    public ICollection<FamilyInvitation> AcceptedInvitations { get; } = new List<FamilyInvitation>();
}
