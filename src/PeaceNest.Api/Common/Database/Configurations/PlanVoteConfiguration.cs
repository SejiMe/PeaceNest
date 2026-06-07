using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class PlanVoteConfiguration : IEntityTypeConfiguration<PlanVote>
{
    public void Configure(EntityTypeBuilder<PlanVote> builder)
    {
        builder.ToTable("plan_votes");

        builder.HasKey(vote => vote.Id);

        builder.Property(vote => vote.Id).HasColumnName("id");
        builder.Property(vote => vote.PlanId).HasColumnName("plan_id");
        builder.Property(vote => vote.UserId).HasColumnName("user_id");
        builder.Property(vote => vote.VoteValue).HasColumnName("vote_value").HasColumnType("vote_value");
        builder.Property(vote => vote.PriorityPoints).HasColumnName("priority_points");
        builder.Property(vote => vote.Note).HasColumnName("note").HasMaxLength(1000);
        builder.Property(vote => vote.CreatedAt).HasColumnName("created_at");
        builder.Property(vote => vote.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(vote => vote.Plan)
            .WithMany(plan => plan.Votes)
            .HasForeignKey(vote => vote.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(vote => vote.User)
            .WithMany(user => user.PlanVotes)
            .HasForeignKey(vote => vote.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(vote => new { vote.PlanId, vote.UserId })
            .IsUnique()
            .HasDatabaseName("ux_plan_votes_plan_id_user_id");

        builder.HasIndex(vote => vote.UserId)
            .HasDatabaseName("ix_plan_votes_user_id");
    }
}
