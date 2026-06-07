namespace PeaceNest.Api.Common.Database.Entities;

public sealed class FamilyPlan : IAuditableEntity, ISoftDeletableEntity, IConcurrencyTrackedEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid? CategoryId { get; set; }

    public PlanCategory? Category { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public PlanType PlanType { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public PlanStatus Status { get; set; } = PlanStatus.Active;

    public int? PriorityRank { get; set; }

    public decimal PriorityScore { get; set; }

    public int ProgressPercent { get; set; }

    public DateOnly? TargetDate { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset? ArchivedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public int Version { get; set; }

    public WantNeedDetails? WantNeedDetails { get; set; }

    public MilestoneDetails? MilestoneDetails { get; set; }

    public ICollection<GoalStep> GoalSteps { get; } = new List<GoalStep>();

    public ICollection<PlanParticipant> Participants { get; } = new List<PlanParticipant>();

    public ICollection<PlanVote> Votes { get; } = new List<PlanVote>();

    public ICollection<Comment> Comments { get; } = new List<Comment>();

    public ICollection<Reaction> Reactions { get; } = new List<Reaction>();

    public ICollection<Memory> Memories { get; } = new List<Memory>();

    public ICollection<Notification> Notifications { get; } = new List<Notification>();

    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();

    public ICollection<RecapItem> RecapItems { get; } = new List<RecapItem>();
}
