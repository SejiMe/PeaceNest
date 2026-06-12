using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace PeaceNest.Api.Common.Database;

public static class ModelBuilderExtensions
{
    public static void ApplyPeaceNestConventions(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property(nameof(IAuditableEntity.CreatedAt)).IsRequired();
                modelBuilder.Entity(entityType.ClrType).Property(nameof(IAuditableEntity.UpdatedAt)).IsRequired();
            }

            if (typeof(IUsesVersion7Guid).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IUsesVersion7Guid.Id))
                    .HasValueGenerator<Version7GuidValueGenerator>();
            }

            if (typeof(ISoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(CreateSoftDeleteFilter(entityType.ClrType));
            }

            if (typeof(IConcurrencyTrackedEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .Property(nameof(IConcurrencyTrackedEntity.Version))
                    .IsConcurrencyToken()
                    .IsRequired();
            }
        }
    }

    private static LambdaExpression CreateSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "entity");
        var deletedAt = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(DateTimeOffset?)],
            parameter,
            Expression.Constant(nameof(ISoftDeletableEntity.DeletedAt)));

        var isNotDeleted = Expression.Equal(deletedAt, Expression.Constant(null, typeof(DateTimeOffset?)));
        return Expression.Lambda(isNotDeleted, parameter);
    }
}
