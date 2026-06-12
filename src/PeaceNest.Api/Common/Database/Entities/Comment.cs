using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Comment : IUsesVersion7Guid, IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public FamilyPlan Plan { get; set; } = null!;

    public Guid AuthorUserId { get; set; }

    public User AuthorUser { get; set; } = null!;

    public Guid? ParentCommentId { get; set; }

    public Comment? ParentComment { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<Comment> Replies { get; } = new List<Comment>();

    public ICollection<Reaction> Reactions { get; } = new List<Reaction>();

    public ICollection<Notification> Notifications { get; } = new List<Notification>();

    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();
}
