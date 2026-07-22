using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class FamilyJoinRequestConfiguration : IEntityTypeConfiguration<FamilyJoinRequest>
{
    public void Configure(EntityTypeBuilder<FamilyJoinRequest> builder)
    {
        builder.ToTable("family_join_requests");
        builder.HasKey(request => request.Id);
        builder.Property(request => request.Id).HasColumnName("id");
        builder.Property(request => request.FamilyId).HasColumnName("family_id");
        builder.Property(request => request.JoinCodeId).HasColumnName("join_code_id");
        builder.Property(request => request.RequesterUserId).HasColumnName("requester_user_id");
        builder.Property(request => request.Status).HasColumnName("status").HasColumnType("family_join_request_status");
        builder.Property(request => request.ExpiresAt).HasColumnName("expires_at");
        builder.Property(request => request.ReviewedByUserId).HasColumnName("reviewed_by_user_id");
        builder.Property(request => request.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(request => request.ApprovedRole).HasColumnName("approved_role").HasColumnType("family_member_role");
        builder.Property(request => request.CreatedAt).HasColumnName("created_at");
        builder.Property(request => request.UpdatedAt).HasColumnName("updated_at");
        builder.Property(request => request.Version).HasColumnName("version");

        builder.HasOne(request => request.Family)
            .WithMany(family => family.JoinRequests)
            .HasForeignKey(request => request.FamilyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.JoinCode)
            .WithMany(code => code.Requests)
            .HasForeignKey(request => request.JoinCodeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.RequesterUser)
            .WithMany(user => user.FamilyJoinRequests)
            .HasForeignKey(request => request.RequesterUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(request => request.ReviewedByUser)
            .WithMany(user => user.ReviewedFamilyJoinRequests)
            .HasForeignKey(request => request.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => new { request.FamilyId, request.RequesterUserId })
            .IsUnique()
            .HasFilter("status = 'pending'")
            .HasDatabaseName("ux_family_join_requests_pending_family_requester");
        builder.HasIndex(request => new { request.FamilyId, request.Status, request.CreatedAt })
            .HasDatabaseName("ix_family_join_requests_family_status_created");
        builder.HasIndex(request => new { request.RequesterUserId, request.Status, request.CreatedAt })
            .HasDatabaseName("ix_family_join_requests_requester_status_created");
        builder.HasIndex(request => request.JoinCodeId).HasDatabaseName("ix_family_join_requests_join_code_id");
        builder.HasIndex(request => request.ReviewedByUserId).HasDatabaseName("ix_family_join_requests_reviewed_by_user_id");
    }
}
