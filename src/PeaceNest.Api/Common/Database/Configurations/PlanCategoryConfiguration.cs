using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class PlanCategoryConfiguration : IEntityTypeConfiguration<PlanCategory>
{
    public void Configure(EntityTypeBuilder<PlanCategory> builder)
    {
        builder.ToTable("plan_categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).HasColumnName("id");
        builder.Property(category => category.FamilyId).HasColumnName("family_id");
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(120);
        builder.Property(category => category.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(category => category.Icon).HasColumnName("icon").HasMaxLength(80);
        builder.Property(category => category.Color).HasColumnName("color").HasMaxLength(40);
        builder.Property(category => category.SortOrder).HasColumnName("sort_order");
        builder.Property(category => category.CreatedAt).HasColumnName("created_at");
        builder.Property(category => category.UpdatedAt).HasColumnName("updated_at");
        builder.Property(category => category.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(category => category.Family)
            .WithMany(family => family.PlanCategories)
            .HasForeignKey(category => category.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(category => new { category.FamilyId, category.SortOrder })
            .HasDatabaseName("ix_plan_categories_family_id_sort_order");

        builder.HasIndex(category => new { category.FamilyId, category.Name })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_plan_categories_active_name");
    }
}
