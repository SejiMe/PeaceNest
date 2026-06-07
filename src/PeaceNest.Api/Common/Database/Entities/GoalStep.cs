namespace PeaceNest.Api.Common.Database.Entities;

public sealed class GoalStep : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public FamilyPlan Plan { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsCompleted { get; set; }

    public Guid? CompletedByUserId { get; set; }

    public User? CompletedByUser { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
