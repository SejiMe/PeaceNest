namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Family : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string PreferredCurrency { get; set; } = "PHP";

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<FamilyMember> Members { get; } = new List<FamilyMember>();

    public ICollection<FamilyInvitation> Invitations { get; } = new List<FamilyInvitation>();

    public ICollection<FamilyJoinCode> JoinCodes { get; } = new List<FamilyJoinCode>();

    public ICollection<FamilyJoinRequest> JoinRequests { get; } = new List<FamilyJoinRequest>();

    public ICollection<FamilyRecoveryCode> RecoveryCodes { get; } = new List<FamilyRecoveryCode>();

    public ICollection<PlanCategory> PlanCategories { get; } = new List<PlanCategory>();

    public ICollection<FamilyPlan> Plans { get; } = new List<FamilyPlan>();

    public ICollection<Memory> Memories { get; } = new List<Memory>();

    public ICollection<Recap> Recaps { get; } = new List<Recap>();

    public ICollection<Notification> Notifications { get; } = new List<Notification>();

    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();
}
