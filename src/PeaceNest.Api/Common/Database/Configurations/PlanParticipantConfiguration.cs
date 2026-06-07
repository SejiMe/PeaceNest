using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class PlanParticipantConfiguration : IEntityTypeConfiguration<PlanParticipant>
{
    public void Configure(EntityTypeBuilder<PlanParticipant> builder)
    {
        builder.ToTable("plan_participants");

        builder.HasKey(participant => participant.Id);

        builder.Property(participant => participant.Id).HasColumnName("id");
        builder.Property(participant => participant.PlanId).HasColumnName("plan_id");
        builder.Property(participant => participant.UserId).HasColumnName("user_id");
        builder.Property(participant => participant.Role).HasColumnName("role").HasMaxLength(120);
        builder.Property(participant => participant.CreatedAt).HasColumnName("created_at");

        builder.HasOne(participant => participant.Plan)
            .WithMany(plan => plan.Participants)
            .HasForeignKey(participant => participant.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(participant => participant.User)
            .WithMany(user => user.PlanParticipations)
            .HasForeignKey(participant => participant.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(participant => new { participant.PlanId, participant.UserId })
            .IsUnique()
            .HasDatabaseName("ux_plan_participants_plan_id_user_id");

        builder.HasIndex(participant => participant.UserId)
            .HasDatabaseName("ix_plan_participants_user_id");
    }
}
