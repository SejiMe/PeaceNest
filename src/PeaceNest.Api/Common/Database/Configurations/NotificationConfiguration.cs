using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Id).HasColumnName("id");
        builder.Property(notification => notification.FamilyId).HasColumnName("family_id");
        builder.Property(notification => notification.RecipientUserId).HasColumnName("recipient_user_id");
        builder.Property(notification => notification.ActorUserId).HasColumnName("actor_user_id");
        builder.Property(notification => notification.Type).HasColumnName("type").HasColumnType("notification_type");
        builder.Property(notification => notification.Title).HasColumnName("title").HasMaxLength(160);
        builder.Property(notification => notification.Body).HasColumnName("body").HasMaxLength(300);
        builder.Property(notification => notification.RelatedPlanId).HasColumnName("related_plan_id");
        builder.Property(notification => notification.RelatedCommentId).HasColumnName("related_comment_id");
        builder.Property(notification => notification.RelatedRecapId).HasColumnName("related_recap_id");
        builder.Property(notification => notification.ReadAt).HasColumnName("read_at");
        builder.Property(notification => notification.CreatedAt).HasColumnName("created_at");
        builder.Property(notification => notification.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(notification => notification.Family)
            .WithMany(family => family.Notifications)
            .HasForeignKey(notification => notification.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.RecipientUser)
            .WithMany(user => user.ReceivedNotifications)
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.ActorUser)
            .WithMany(user => user.TriggeredNotifications)
            .HasForeignKey(notification => notification.ActorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.RelatedPlan)
            .WithMany(plan => plan.Notifications)
            .HasForeignKey(notification => notification.RelatedPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.RelatedComment)
            .WithMany(comment => comment.Notifications)
            .HasForeignKey(notification => notification.RelatedCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(notification => notification.RelatedRecap)
            .WithMany(recap => recap.Notifications)
            .HasForeignKey(notification => notification.RelatedRecapId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(notification => new { notification.RecipientUserId, notification.ReadAt, notification.CreatedAt })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_notifications_active_recipient_read_created");

        builder.HasIndex(notification => new { notification.FamilyId, notification.CreatedAt })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_notifications_active_family_created");

        builder.HasIndex(notification => notification.ActorUserId)
            .HasDatabaseName("ix_notifications_actor_user_id");

        builder.HasIndex(notification => notification.RelatedPlanId)
            .HasDatabaseName("ix_notifications_related_plan_id");

        builder.HasIndex(notification => notification.RelatedCommentId)
            .HasDatabaseName("ix_notifications_related_comment_id");

        builder.HasIndex(notification => notification.RelatedRecapId)
            .HasDatabaseName("ix_notifications_related_recap_id");
    }
}
