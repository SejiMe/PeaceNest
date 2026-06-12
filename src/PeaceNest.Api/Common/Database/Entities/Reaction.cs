using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Reaction : IUsesVersion7Guid
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid? PlanId { get; set; }

    public FamilyPlan? Plan { get; set; }

    public Guid? CommentId { get; set; }

    public Comment? Comment { get; set; }

    public string ReactionType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
