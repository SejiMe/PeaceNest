using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class PlanParticipant : IUsesVersion7Guid
{
    public Guid Id { get; set; }

    public Guid PlanId { get; set; }

    public FamilyPlan Plan { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public string? Role { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
