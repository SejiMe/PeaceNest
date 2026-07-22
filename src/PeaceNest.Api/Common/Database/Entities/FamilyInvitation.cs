using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class FamilyInvitation : IUsesVersion7Guid, IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public string InvitedEmail { get; set; } = string.Empty;

    public FamilyMemberRole InvitedRole { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public string? InvitationCodeHash { get; set; }

    public FamilyInvitationStatus Status { get; set; } = FamilyInvitationStatus.Pending;

    public DateTimeOffset ExpiresAt { get; set; }

    public Guid? AcceptedByUserId { get; set; }

    public User? AcceptedByUser { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
