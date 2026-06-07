using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class MilestoneDetailsConfiguration : IEntityTypeConfiguration<MilestoneDetails>
{
    public void Configure(EntityTypeBuilder<MilestoneDetails> builder)
    {
        builder.ToTable("milestone_details");

        builder.HasKey(details => details.PlanId);

        builder.Property(details => details.PlanId).HasColumnName("plan_id");
        builder.Property(details => details.MilestoneType).HasColumnName("milestone_type").HasMaxLength(120);
        builder.Property(details => details.CelebrationNotes).HasColumnName("celebration_notes").HasMaxLength(1000);
        builder.Property(details => details.ReflectionPrompt).HasColumnName("reflection_prompt").HasMaxLength(1000);
        builder.Property(details => details.IncludeInRecap).HasColumnName("include_in_recap");

        builder.HasOne(details => details.Plan)
            .WithOne(plan => plan.MilestoneDetails)
            .HasForeignKey<MilestoneDetails>(details => details.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
