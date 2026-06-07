using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");

        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.Id).HasColumnName("id");
        builder.Property(activity => activity.FamilyId).HasColumnName("family_id");
        builder.Property(activity => activity.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(activity => activity.ActivityType).HasColumnName("activity_type").HasColumnType("activity_type");
        builder.Property(activity => activity.PlanId).HasColumnName("plan_id");
        builder.Property(activity => activity.CommentId).HasColumnName("comment_id");
        builder.Property(activity => activity.RecapId).HasColumnName("recap_id");
        builder.Property(activity => activity.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(JsonDocumentMapping.Converter)
            .Metadata.SetValueComparer(JsonDocumentMapping.Comparer);
        builder.Property(activity => activity.CreatedAt).HasColumnName("created_at");

        builder.HasOne(activity => activity.Family)
            .WithMany(family => family.ActivityLogs)
            .HasForeignKey(activity => activity.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(activity => activity.ActorUser)
            .WithMany(user => user.ActivityLogs)
            .HasForeignKey(activity => activity.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(activity => activity.Plan)
            .WithMany(plan => plan.ActivityLogs)
            .HasForeignKey(activity => activity.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(activity => activity.Comment)
            .WithMany(comment => comment.ActivityLogs)
            .HasForeignKey(activity => activity.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(activity => activity.Recap)
            .WithMany(recap => recap.ActivityLogs)
            .HasForeignKey(activity => activity.RecapId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(activity => new { activity.FamilyId, activity.CreatedAt })
            .HasDatabaseName("ix_activity_logs_family_id_created_at");

        builder.HasIndex(activity => activity.ActorUserId)
            .HasDatabaseName("ix_activity_logs_actor_user_id");

        builder.HasIndex(activity => activity.PlanId)
            .HasDatabaseName("ix_activity_logs_plan_id");

        builder.HasIndex(activity => activity.CommentId)
            .HasDatabaseName("ix_activity_logs_comment_id");

        builder.HasIndex(activity => activity.RecapId)
            .HasDatabaseName("ix_activity_logs_recap_id");
    }
}
