using System.Text.Json;
using PeaceNest.Api.Common.Database;

namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Recap : IUsesVersion7Guid, IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public RecapPeriodType PeriodType { get; set; } = RecapPeriodType.Monthly;

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public JsonDocument Stats { get; set; } = JsonDocument.Parse("{}");

    public Guid GeneratedByUserId { get; set; }

    public User GeneratedByUser { get; set; } = null!;

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<RecapItem> Items { get; } = new List<RecapItem>();

    public ICollection<Notification> Notifications { get; } = new List<Notification>();

    public ICollection<ActivityLog> ActivityLogs { get; } = new List<ActivityLog>();
}
