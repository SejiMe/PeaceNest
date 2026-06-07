namespace PeaceNest.Api.Common.Database.Entities;

public sealed class RecapItem
{
    public Guid Id { get; set; }

    public Guid RecapId { get; set; }

    public Recap Recap { get; set; } = null!;

    public Guid? PlanId { get; set; }

    public FamilyPlan? Plan { get; set; }

    public Guid? MemoryId { get; set; }

    public Memory? Memory { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
