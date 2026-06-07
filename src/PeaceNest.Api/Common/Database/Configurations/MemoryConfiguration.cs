using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> builder)
    {
        builder.ToTable("memories");

        builder.HasKey(memory => memory.Id);

        builder.Property(memory => memory.Id).HasColumnName("id");
        builder.Property(memory => memory.FamilyId).HasColumnName("family_id");
        builder.Property(memory => memory.PlanId).HasColumnName("plan_id");
        builder.Property(memory => memory.UploadedByUserId).HasColumnName("uploaded_by_user_id");
        builder.Property(memory => memory.Caption).HasColumnName("caption").HasMaxLength(1000);
        builder.Property(memory => memory.MediaType).HasColumnName("media_type").HasMaxLength(80);
        builder.Property(memory => memory.StorageProvider).HasColumnName("storage_provider").HasMaxLength(80);
        builder.Property(memory => memory.StoragePath).HasColumnName("storage_path").HasMaxLength(2048);
        builder.Property(memory => memory.ThumbnailPath).HasColumnName("thumbnail_path").HasMaxLength(2048);
        builder.Property(memory => memory.TakenAt).HasColumnName("taken_at");
        builder.Property(memory => memory.CreatedAt).HasColumnName("created_at");
        builder.Property(memory => memory.UpdatedAt).HasColumnName("updated_at");
        builder.Property(memory => memory.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(memory => memory.Family)
            .WithMany(family => family.Memories)
            .HasForeignKey(memory => memory.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(memory => memory.Plan)
            .WithMany(plan => plan.Memories)
            .HasForeignKey(memory => memory.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(memory => memory.UploadedByUser)
            .WithMany(user => user.UploadedMemories)
            .HasForeignKey(memory => memory.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(memory => new { memory.FamilyId, memory.CreatedAt })
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ix_memories_active_family_id_created_at");

        builder.HasIndex(memory => memory.PlanId)
            .HasDatabaseName("ix_memories_plan_id");

        builder.HasIndex(memory => memory.UploadedByUserId)
            .HasDatabaseName("ix_memories_uploaded_by_user_id");
    }
}
