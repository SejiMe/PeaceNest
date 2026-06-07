using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyPlanConfiguration : IEntityTypeConfiguration<FamilyPlan>
{
    public void Configure(EntityTypeBuilder<FamilyPlan> builder)
    {
        builder.ToTable("family_plans");

        builder.HasKey(plan => plan.Id);

        builder.Property(plan => plan.Id).HasColumnName("id");
        builder.Property(plan => plan.FamilyId).HasColumnName("family_id");
        builder.Property(plan => plan.CategoryId).HasColumnName("category_id");
        builder.Property(plan => plan.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(plan => plan.PlanType).HasColumnName("plan_type").HasColumnType("plan_type");
        builder.Property(plan => plan.Title).HasColumnName("title").HasMaxLength(180);
        builder.Property(plan => plan.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(plan => plan.Status).HasColumnName("status").HasColumnType("plan_status");
        builder.Property(plan => plan.PriorityRank).HasColumnName("priority_rank");
        builder.Property(plan => plan.PriorityScore).HasColumnName("priority_score").HasPrecision(9, 2);
        builder.Property(plan => plan.ProgressPercent).HasColumnName("progress_percent");
        builder.Property(plan => plan.TargetDate).HasColumnName("target_date").HasColumnType("date");
        builder.Property(plan => plan.StartedAt).HasColumnName("started_at");
        builder.Property(plan => plan.CompletedAt).HasColumnName("completed_at");
        builder.Property(plan => plan.ArchivedAt).HasColumnName("archived_at");
        builder.Property(plan => plan.CreatedAt).HasColumnName("created_at");
        builder.Property(plan => plan.UpdatedAt).HasColumnName("updated_at");
        builder.Property(plan => plan.DeletedAt).HasColumnName("deleted_at");
        builder.Property(plan => plan.Version).HasColumnName("version");

        builder.HasOne(plan => plan.Family)
            .WithMany(family => family.Plans)
            .HasForeignKey(plan => plan.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(plan => plan.Category)
            .WithMany(category => category.Plans)
            .HasForeignKey(plan => plan.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(plan => plan.CreatedByUser)
            .WithMany(user => user.CreatedPlans)
            .HasForeignKey(plan => plan.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(plan => new { plan.FamilyId, plan.PlanType, plan.Status })
            .HasDatabaseName("ix_family_plans_family_id_plan_type_status");

        builder.HasIndex(plan => new { plan.FamilyId, plan.PriorityRank })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_family_plans_active_priority_rank");

        builder.HasIndex(plan => plan.CreatedByUserId)
            .HasDatabaseName("ix_family_plans_created_by_user_id");

        builder.HasIndex(plan => plan.CategoryId)
            .HasDatabaseName("ix_family_plans_category_id");
    }
}
