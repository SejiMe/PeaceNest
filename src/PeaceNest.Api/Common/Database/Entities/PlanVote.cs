namespace PeaceNest.Api.Common.Database.Entities;

public sealed class PlanVote : IAuditableEntity
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public FamilyPlan Plan { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public VoteValue VoteValue { get; set; }

    public int PriorityPoints { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
