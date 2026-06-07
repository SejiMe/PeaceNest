namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Notification : ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid RecipientUserId { get; set; }

    public User RecipientUser { get; set; } = null!;

    public Guid? ActorUserId { get; set; }

    public User? ActorUser { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public Guid? RelatedPlanId { get; set; }

    public FamilyPlan? RelatedPlan { get; set; }

    public Guid? RelatedCommentId { get; set; }

    public Comment? RelatedComment { get; set; }

    public Guid? RelatedRecapId { get; set; }

    public Recap? RelatedRecap { get; set; }

    public DateTimeOffset? ReadAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }
}
