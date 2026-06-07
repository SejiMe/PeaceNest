using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");

        builder.HasKey(comment => comment.Id);

        builder.Property(comment => comment.Id).HasColumnName("id");
        builder.Property(comment => comment.PlanId).HasColumnName("plan_id");
        builder.Property(comment => comment.AuthorUserId).HasColumnName("author_user_id");
        builder.Property(comment => comment.ParentCommentId).HasColumnName("parent_comment_id");
        builder.Property(comment => comment.Body).HasColumnName("body").HasMaxLength(4000);
        builder.Property(comment => comment.CreatedAt).HasColumnName("created_at");
        builder.Property(comment => comment.UpdatedAt).HasColumnName("updated_at");
        builder.Property(comment => comment.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(comment => comment.Plan)
            .WithMany(plan => plan.Comments)
            .HasForeignKey(comment => comment.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(comment => comment.AuthorUser)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(comment => comment.ParentComment)
            .WithMany(comment => comment.Replies)
            .HasForeignKey(comment => comment.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(comment => new { comment.PlanId, comment.CreatedAt })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_comments_active_plan_id_created_at");

        builder.HasIndex(comment => comment.AuthorUserId)
            .HasDatabaseName("ix_comments_author_user_id");

        builder.HasIndex(comment => comment.ParentCommentId)
            .HasDatabaseName("ix_comments_parent_comment_id");
    }
}
