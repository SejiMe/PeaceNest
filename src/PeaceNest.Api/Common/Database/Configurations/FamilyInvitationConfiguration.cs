using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyInvitationConfiguration : IEntityTypeConfiguration<FamilyInvitation>
{
    public void Configure(EntityTypeBuilder<FamilyInvitation> builder)
    {
        builder.ToTable("family_invitations");

        builder.HasKey(invitation => invitation.Id);

        builder.Property(invitation => invitation.Id).HasColumnName("id");
        builder.Property(invitation => invitation.FamilyId).HasColumnName("family_id");
        builder.Property(invitation => invitation.InvitedEmail)
            .HasColumnName("invited_email")
            .HasColumnType("citext")
            .HasMaxLength(320);
        builder.Property(invitation => invitation.InvitedRole)
            .HasColumnName("invited_role")
            .HasColumnType("family_member_role");
        builder.Property(invitation => invitation.TokenHash).HasColumnName("token_hash").HasMaxLength(128);
        builder.Property(invitation => invitation.InvitationCodeHash)
            .HasColumnName("invitation_code_hash")
            .HasMaxLength(64);
        builder.Property(invitation => invitation.Status).HasColumnName("status").HasColumnType("family_invitation_status");
        builder.Property(invitation => invitation.ExpiresAt).HasColumnName("expires_at");
        builder.Property(invitation => invitation.AcceptedByUserId).HasColumnName("accepted_by_user_id");
        builder.Property(invitation => invitation.AcceptedAt).HasColumnName("accepted_at");
        builder.Property(invitation => invitation.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(invitation => invitation.CreatedAt).HasColumnName("created_at");
        builder.Property(invitation => invitation.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(invitation => invitation.Family)
            .WithMany(family => family.Invitations)
            .HasForeignKey(invitation => invitation.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(invitation => invitation.CreatedByUser)
            .WithMany(user => user.CreatedInvitations)
            .HasForeignKey(invitation => invitation.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(invitation => invitation.AcceptedByUser)
            .WithMany(user => user.AcceptedInvitations)
            .HasForeignKey(invitation => invitation.AcceptedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(invitation => invitation.TokenHash)
            .IsUnique()
            .HasDatabaseName("ux_family_invitations_token_hash");

        builder.HasIndex(invitation => invitation.InvitationCodeHash)
            .IsUnique()
            .HasFilter("invitation_code_hash IS NOT NULL")
            .HasDatabaseName("ux_family_invitations_invitation_code_hash");

        builder.HasIndex(invitation => new { invitation.FamilyId, invitation.InvitedEmail })
            .HasFilter("status = 'pending'::family_invitation_status")
            .HasDatabaseName("ix_family_invitations_pending_email");

        builder.HasIndex(invitation => invitation.CreatedByUserId)
            .HasDatabaseName("ix_family_invitations_created_by_user_id");

        builder.HasIndex(invitation => invitation.AcceptedByUserId)
            .HasDatabaseName("ix_family_invitations_accepted_by_user_id");
    }
}
