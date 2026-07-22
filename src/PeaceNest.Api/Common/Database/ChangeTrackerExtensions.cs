using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PeaceNest.Api.Common.Database;

public static class ChangeTrackerExtensions
{
    public static void ApplyPeaceNestPersistenceConventions(
        this ChangeTracker changeTracker,
        DateTimeOffset utcNow,
        IReadOnlySet<object>? permanentDeletes = null)
    {
        foreach (var entry in changeTracker.Entries())
        {
            ApplyVersion7GuidConvention(entry);
            ApplySoftDeleteConvention(entry, utcNow, permanentDeletes);
            ApplyAuditConvention(entry, utcNow);
            ApplyConcurrencyConvention(entry);
        }
    }

    private static void ApplyVersion7GuidConvention(EntityEntry entry)
    {
        if (entry.State is EntityState.Added &&
            entry.Entity is IUsesVersion7Guid timeOrderedEntity &&
            timeOrderedEntity.Id == Guid.Empty)
        {
            timeOrderedEntity.Id = Guid.CreateVersion7();
        }
    }

    private static void ApplySoftDeleteConvention(
        EntityEntry entry,
        DateTimeOffset utcNow,
        IReadOnlySet<object>? permanentDeletes)
    {
        if (entry.State is not EntityState.Deleted || entry.Entity is not ISoftDeletableEntity softDeletable)
        {
            return;
        }

        if (permanentDeletes?.Contains(entry.Entity) is true)
        {
            return;
        }

        entry.State = EntityState.Modified;
        softDeletable.DeletedAt ??= utcNow;
    }

    private static void ApplyAuditConvention(EntityEntry entry, DateTimeOffset utcNow)
    {
        if (entry.Entity is not IAuditableEntity auditable)
        {
            return;
        }

        if (entry.State is EntityState.Added)
        {
            auditable.CreatedAt = utcNow;
            auditable.UpdatedAt = utcNow;
            return;
        }

        if (entry.State is EntityState.Modified)
        {
            auditable.UpdatedAt = utcNow;
            entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
        }
    }

    private static void ApplyConcurrencyConvention(EntityEntry entry)
    {
        if (entry.Entity is not IConcurrencyTrackedEntity concurrencyTracked)
        {
            return;
        }

        if (entry.State is EntityState.Added)
        {
            concurrencyTracked.Version = 1;
        }

        if (entry.State is EntityState.Modified)
        {
            concurrencyTracked.Version++;
        }
    }
}
