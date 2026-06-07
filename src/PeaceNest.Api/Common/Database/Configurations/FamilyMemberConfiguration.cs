using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id).HasColumnName("id");
        builder.Property(member => member.FamilyId).HasColumnName("family_id");
        builder.Property(member => member.UserId).HasColumnName("user_id");
        builder.Property(member => member.Role).HasColumnName("role").HasColumnType("family_member_role");
        builder.Property(member => member.Status).HasColumnName("status").HasColumnType("family_member_status");
        builder.Property(member => member.Nickname).HasColumnName("nickname").HasMaxLength(120);
        builder.Property(member => member.JoinedAt).HasColumnName("joined_at");
        builder.Property(member => member.RemovedAt).HasColumnName("removed_at");
        builder.Property(member => member.CreatedAt).HasColumnName("created_at");
        builder.Property(member => member.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(member => member.Family)
            .WithMany(family => family.Members)
            .HasForeignKey(member => member.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(member => member.User)
            .WithMany(user => user.FamilyMemberships)
            .HasForeignKey(member => member.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(member => new { member.FamilyId, member.UserId })
            .IsUnique()
            .HasDatabaseName("ux_family_members_family_id_user_id");

        builder.HasIndex(member => member.UserId)
            .HasDatabaseName("ix_family_members_user_id");

        builder.HasIndex(member => new { member.FamilyId, member.Status })
            .HasDatabaseName("ix_family_members_family_id_status");
    }
}
