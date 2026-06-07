using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class WantNeedDetailsConfiguration : IEntityTypeConfiguration<WantNeedDetails>
{
    public void Configure(EntityTypeBuilder<WantNeedDetails> builder)
    {
        builder.ToTable("want_need_details");

        builder.HasKey(details => details.PlanId);

        builder.Property(details => details.PlanId).HasColumnName("plan_id");
        builder.Property(details => details.Kind).HasColumnName("kind").HasColumnType("want_need_kind");
        builder.Property(details => details.EstimatedCostAmount)
            .HasColumnName("estimated_cost_amount")
            .HasPrecision(12, 2);
        builder.Property(details => details.EstimatedCostCurrency)
            .HasColumnName("estimated_cost_currency")
            .HasColumnType("char(3)")
            .HasMaxLength(3);
        builder.Property(details => details.UrgencyLevel).HasColumnName("urgency_level").HasColumnType("score_level");
        builder.Property(details => details.ImportanceLevel).HasColumnName("importance_level").HasColumnType("score_level");
        builder.Property(details => details.EmotionalValueLevel)
            .HasColumnName("emotional_value_level")
            .HasColumnType("score_level");
        builder.Property(details => details.DesiredByDate).HasColumnName("desired_by_date").HasColumnType("date");
        builder.Property(details => details.FundingNotes).HasColumnName("funding_notes").HasMaxLength(1000);

        builder.HasOne(details => details.Plan)
            .WithOne(plan => plan.WantNeedDetails)
            .HasForeignKey<WantNeedDetails>(details => details.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
