using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class ReactionConfiguration : IEntityTypeConfiguration<Reaction>
{
    public void Configure(EntityTypeBuilder<Reaction> builder)
    {
        builder.ToTable("reactions");

        builder.HasKey(reaction => reaction.Id);

        builder.Property(reaction => reaction.Id).HasColumnName("id");
        builder.Property(reaction => reaction.UserId).HasColumnName("user_id");
        builder.Property(reaction => reaction.PlanId).HasColumnName("plan_id");
        builder.Property(reaction => reaction.CommentId).HasColumnName("comment_id");
        builder.Property(reaction => reaction.ReactionType).HasColumnName("reaction_type").HasMaxLength(80);
        builder.Property(reaction => reaction.CreatedAt).HasColumnName("created_at");

        builder.HasOne(reaction => reaction.User)
            .WithMany(user => user.Reactions)
            .HasForeignKey(reaction => reaction.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reaction => reaction.Plan)
            .WithMany(plan => plan.Reactions)
            .HasForeignKey(reaction => reaction.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(reaction => reaction.Comment)
            .WithMany(comment => comment.Reactions)
            .HasForeignKey(reaction => reaction.CommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(reaction => new { reaction.PlanId, reaction.UserId, reaction.ReactionType })
            .IsUnique()
            .HasFilter("plan_id IS NOT NULL")
            .HasDatabaseName("ux_reactions_plan_user_type");

        builder.HasIndex(reaction => new { reaction.CommentId, reaction.UserId, reaction.ReactionType })
            .IsUnique()
            .HasFilter("comment_id IS NOT NULL")
            .HasDatabaseName("ux_reactions_comment_user_type");

        builder.HasIndex(reaction => reaction.UserId)
            .HasDatabaseName("ix_reactions_user_id");
    }
}
