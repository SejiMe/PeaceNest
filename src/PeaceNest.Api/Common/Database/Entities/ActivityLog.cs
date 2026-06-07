using System.Text.Json;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class ActivityLog
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid ActorUserId { get; set; }

    public User ActorUser { get; set; } = null!;

    public ActivityType ActivityType { get; set; }

    public Guid? PlanId { get; set; }

    public FamilyPlan? Plan { get; set; }

    public Guid? CommentId { get; set; }

    public Comment? Comment { get; set; }

    public Guid? RecapId { get; set; }

    public Recap? Recap { get; set; }

    public JsonDocument Metadata { get; set; } = JsonDocument.Parse("{}");

    public DateTimeOffset CreatedAt { get; set; }
}
