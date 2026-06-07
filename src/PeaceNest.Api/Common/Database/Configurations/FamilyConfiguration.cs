using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(family => family.Id);

        builder.Property(family => family.Id).HasColumnName("id");
        builder.Property(family => family.Name).HasColumnName("name").HasMaxLength(120);
        builder.Property(family => family.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(family => family.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(family => family.CreatedAt).HasColumnName("created_at");
        builder.Property(family => family.UpdatedAt).HasColumnName("updated_at");
        builder.Property(family => family.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(family => family.CreatedByUser)
            .WithMany(user => user.CreatedFamilies)
            .HasForeignKey(family => family.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(family => family.CreatedByUserId)
            .HasDatabaseName("ix_families_created_by_user_id");
    }
}
