using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class RecapConfiguration : IEntityTypeConfiguration<Recap>
{
    public void Configure(EntityTypeBuilder<Recap> builder)
    {
        builder.ToTable("recaps");

        builder.HasKey(recap => recap.Id);

        builder.Property(recap => recap.Id).HasColumnName("id");
        builder.Property(recap => recap.FamilyId).HasColumnName("family_id");
        builder.Property(recap => recap.PeriodType).HasColumnName("period_type").HasColumnType("recap_period_type");
        builder.Property(recap => recap.PeriodStart).HasColumnName("period_start").HasColumnType("date");
        builder.Property(recap => recap.PeriodEnd).HasColumnName("period_end").HasColumnType("date");
        builder.Property(recap => recap.Title).HasColumnName("title").HasMaxLength(180);
        builder.Property(recap => recap.Summary).HasColumnName("summary").HasMaxLength(4000);
        builder.Property(recap => recap.Stats)
            .HasColumnName("stats")
            .HasColumnType("jsonb")
            .HasConversion(JsonDocumentMapping.Converter)
            .Metadata.SetValueComparer(JsonDocumentMapping.Comparer);
        builder.Property(recap => recap.GeneratedByUserId).HasColumnName("generated_by_user_id");
        builder.Property(recap => recap.PublishedAt).HasColumnName("published_at");
        builder.Property(recap => recap.CreatedAt).HasColumnName("created_at");
        builder.Property(recap => recap.UpdatedAt).HasColumnName("updated_at");
        builder.Property(recap => recap.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(recap => recap.Family)
            .WithMany(family => family.Recaps)
            .HasForeignKey(recap => recap.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(recap => recap.GeneratedByUser)
            .WithMany(user => user.GeneratedRecaps)
            .HasForeignKey(recap => recap.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(recap => new { recap.FamilyId, recap.PeriodType, recap.PeriodStart })
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ux_recaps_active_family_period");

        builder.HasIndex(recap => recap.GeneratedByUserId)
            .HasDatabaseName("ix_recaps_generated_by_user_id");
    }
}
