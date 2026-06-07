using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class RecapItemConfiguration : IEntityTypeConfiguration<RecapItem>
{
    public void Configure(EntityTypeBuilder<RecapItem> builder)
    {
        builder.ToTable("recap_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.RecapId).HasColumnName("recap_id");
        builder.Property(item => item.PlanId).HasColumnName("plan_id");
        builder.Property(item => item.MemoryId).HasColumnName("memory_id");
        builder.Property(item => item.ItemType).HasColumnName("item_type").HasMaxLength(120);
        builder.Property(item => item.Title).HasColumnName("title").HasMaxLength(180);
        builder.Property(item => item.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(item => item.SortOrder).HasColumnName("sort_order");
        builder.Property(item => item.CreatedAt).HasColumnName("created_at");

        builder.HasOne(item => item.Recap)
            .WithMany(recap => recap.Items)
            .HasForeignKey(item => item.RecapId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.Plan)
            .WithMany(plan => plan.RecapItems)
            .HasForeignKey(item => item.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.Memory)
            .WithMany(memory => memory.RecapItems)
            .HasForeignKey(item => item.MemoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new { item.RecapId, item.SortOrder })
            .HasDatabaseName("ix_recap_items_recap_id_sort_order");

        builder.HasIndex(item => item.PlanId)
            .HasDatabaseName("ix_recap_items_plan_id");

        builder.HasIndex(item => item.MemoryId)
            .HasDatabaseName("ix_recap_items_memory_id");
    }
}
