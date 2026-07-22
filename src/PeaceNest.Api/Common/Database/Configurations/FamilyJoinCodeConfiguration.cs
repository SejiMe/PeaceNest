using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyJoinCodeConfiguration : IEntityTypeConfiguration<FamilyJoinCode>
{
    public void Configure(EntityTypeBuilder<FamilyJoinCode> builder)
    {
        builder.ToTable("family_join_codes", table =>
        {
            table.HasCheckConstraint("ck_family_join_codes_request_count", "request_count >= 0 AND request_count <= max_requests");
            table.HasCheckConstraint("ck_family_join_codes_max_requests", "max_requests > 0");
        });

        builder.HasKey(code => code.Id);
        builder.Property(code => code.Id).HasColumnName("id");
        builder.Property(code => code.FamilyId).HasColumnName("family_id");
        builder.Property(code => code.CodeHash).HasColumnName("code_hash").HasMaxLength(64);
        builder.Property(code => code.Status).HasColumnName("status").HasColumnType("family_join_code_status");
        builder.Property(code => code.RequestCount).HasColumnName("request_count");
        builder.Property(code => code.MaxRequests).HasColumnName("max_requests");
        builder.Property(code => code.ExpiresAt).HasColumnName("expires_at");
        builder.Property(code => code.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(code => code.RevokedByUserId).HasColumnName("revoked_by_user_id");
        builder.Property(code => code.RevokedAt).HasColumnName("revoked_at");
        builder.Property(code => code.CreatedAt).HasColumnName("created_at");
        builder.Property(code => code.UpdatedAt).HasColumnName("updated_at");
        builder.Property(code => code.Version).HasColumnName("version");

        builder.HasOne(code => code.Family)
            .WithMany(family => family.JoinCodes)
            .HasForeignKey(code => code.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(code => code.CreatedByUser)
            .WithMany(user => user.CreatedJoinCodes)
            .HasForeignKey(code => code.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(code => code.RevokedByUser)
            .WithMany(user => user.RevokedJoinCodes)
            .HasForeignKey(code => code.RevokedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(code => code.CodeHash).IsUnique().HasDatabaseName("ux_family_join_codes_code_hash");
        builder.HasIndex(code => code.FamilyId)
            .IsUnique()
            .HasFilter("status = 'active'")
            .HasDatabaseName("ux_family_join_codes_active_family");
        builder.HasIndex(code => code.CreatedByUserId).HasDatabaseName("ix_family_join_codes_created_by_user_id");
        builder.HasIndex(code => code.RevokedByUserId).HasDatabaseName("ix_family_join_codes_revoked_by_user_id");
    }
}
