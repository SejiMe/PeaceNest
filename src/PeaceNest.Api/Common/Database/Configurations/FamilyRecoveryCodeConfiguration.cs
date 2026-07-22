using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyRecoveryCodeConfiguration : IEntityTypeConfiguration<FamilyRecoveryCode>
{
    public void Configure(EntityTypeBuilder<FamilyRecoveryCode> builder)
    {
        builder.ToTable("family_recovery_codes", table =>
        {
            table.HasCheckConstraint(
                "ck_family_recovery_codes_expiry",
                "expires_at > created_at");
        });

        builder.HasKey(code => code.Id);

        builder.Property(code => code.Id).HasColumnName("id");
        builder.Property(code => code.FamilyId).HasColumnName("family_id");
        builder.Property(code => code.CreatorUserId).HasColumnName("creator_user_id");
        builder.Property(code => code.CodeHash).HasColumnName("code_hash").HasMaxLength(64);
        builder.Property(code => code.Status).HasColumnName("status").HasColumnType("family_recovery_code_status");
        builder.Property(code => code.ExpiresAt).HasColumnName("expires_at");
        builder.Property(code => code.UsedAt).HasColumnName("used_at");
        builder.Property(code => code.PurgeClaimedAt).HasColumnName("purge_claimed_at");
        builder.Property(code => code.CreatedAt).HasColumnName("created_at");
        builder.Property(code => code.UpdatedAt).HasColumnName("updated_at");
        builder.Property(code => code.Version).HasColumnName("version").IsConcurrencyToken();

        builder.HasOne(code => code.Family)
            .WithMany(family => family.RecoveryCodes)
            .HasForeignKey(code => code.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(code => code.CreatorUser)
            .WithMany(user => user.FamilyRecoveryCodes)
            .HasForeignKey(code => code.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(code => code.CodeHash)
            .IsUnique()
            .HasDatabaseName("ux_family_recovery_codes_code_hash");

        builder.HasIndex(code => code.FamilyId)
            .IsUnique()
            .HasFilter("status = 'active'")
            .HasDatabaseName("ux_family_recovery_codes_active_family_id");

        builder.HasIndex(code => new { code.Status, code.ExpiresAt, code.PurgeClaimedAt })
            .HasFilter("status = 'active'")
            .HasDatabaseName("ix_family_recovery_codes_active_expiry_claim");

        builder.HasIndex(code => code.CreatorUserId)
            .HasDatabaseName("ix_family_recovery_codes_creator_user_id");
    }
}
