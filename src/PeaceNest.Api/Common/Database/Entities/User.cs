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

    public ICollection<FamilyPlan> CreatedPlans { get; } = new List<FamilyPlan>();

    public ICollection<GoalStep> CompletedGoalSteps { get; } = new List<GoalStep>();

    public ICollection<PlanParticipant> PlanParticipations { get; } = new List<PlanParticipant>();

    public ICollection<PlanVote> PlanVotes { get; } = new List<PlanVote>();

    public ICollection<Comment> Comments { get; } = new List<Comment>();

    public ICollection<Reaction> Reactions { get; } = new List<Reaction>();

    public ICollection<Memory> UploadedMemories { get; } = new List<Memory>();

    public ICollection<Recap> GeneratedRecaps { get; } = new List<Recap>();

    public ICollection<Notification> ReceivedNotifications { get; } = new List<Notification>();

    public ICollection<Notification> TriggeredNotifications { get; } = new List<Notification>();

    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();
}
