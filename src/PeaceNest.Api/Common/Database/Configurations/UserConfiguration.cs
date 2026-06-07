using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Database.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id).HasColumnName("id");
        builder.Property(user => user.SupabaseUserId).HasColumnName("supabase_user_id");
        builder.Property(user => user.Email).HasColumnName("email").HasColumnType("citext").HasMaxLength(320);
        builder.Property(user => user.DisplayName).HasColumnName("display_name").HasMaxLength(200);
        builder.Property(user => user.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(2048);
        builder.Property(user => user.Timezone).HasColumnName("timezone").HasMaxLength(100);
        builder.Property(user => user.LastSeenAt).HasColumnName("last_seen_at");
        builder.Property(user => user.CreatedAt).HasColumnName("created_at");
        builder.Property(user => user.UpdatedAt).HasColumnName("updated_at");
        builder.Property(user => user.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(user => user.SupabaseUserId)
            .IsUnique()
            .HasDatabaseName("ux_users_supabase_user_id");

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasFilter("deleted_at IS NULL")
            .HasDatabaseName("ux_users_active_email");
    }
}
