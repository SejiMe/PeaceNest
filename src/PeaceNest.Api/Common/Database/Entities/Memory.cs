namespace PeaceNest.Api.Common.Database.Entities;

public sealed class Memory : IAuditableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid FamilyId { get; set; }

    public Family Family { get; set; } = null!;

    public Guid? PlanId { get; set; }

    public FamilyPlan? Plan { get; set; }

    public Guid UploadedByUserId { get; set; }

    public User UploadedByUser { get; set; } = null!;

    public string? Caption { get; set; }

    public string MediaType { get; set; } = string.Empty;

    public string StorageProvider { get; set; } = string.Empty;

    public string StoragePath { get; set; } = string.Empty;

    public string? ThumbnailPath { get; set; }

    public DateTimeOffset? TakenAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<RecapItem> RecapItems { get; } = new List<RecapItem>();
}
