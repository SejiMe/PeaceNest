using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class GoalStepConfiguration : IEntityTypeConfiguration<GoalStep>
{
    public void Configure(EntityTypeBuilder<GoalStep> builder)
    {
        builder.ToTable("goal_steps");

        builder.HasKey(step => step.Id);

        builder.Property(step => step.Id).HasColumnName("id");
        builder.Property(step => step.PlanId).HasColumnName("plan_id");
        builder.Property(step => step.Title).HasColumnName("title").HasMaxLength(180);
        builder.Property(step => step.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(step => step.SortOrder).HasColumnName("sort_order");
        builder.Property(step => step.IsCompleted).HasColumnName("is_completed");
        builder.Property(step => step.CompletedByUserId).HasColumnName("completed_by_user_id");
        builder.Property(step => step.CompletedAt).HasColumnName("completed_at");
        builder.Property(step => step.CreatedAt).HasColumnName("created_at");
        builder.Property(step => step.UpdatedAt).HasColumnName("updated_at");
        builder.Property(step => step.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(step => step.Plan)
            .WithMany(plan => plan.GoalSteps)
            .HasForeignKey(step => step.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(step => step.CompletedByUser)
            .WithMany(user => user.CompletedGoalSteps)
            .HasForeignKey(step => step.CompletedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(step => new { step.PlanId, step.SortOrder })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_goal_steps_active_plan_id_sort_order");

        builder.HasIndex(step => step.CompletedByUserId)
            .HasDatabaseName("ix_goal_steps_completed_by_user_id");
    }
}
