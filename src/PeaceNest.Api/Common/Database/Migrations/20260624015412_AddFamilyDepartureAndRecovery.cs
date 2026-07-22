using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PeaceNest.Api.Common.Database.Entities;

#nullable disable

namespace PeaceNest.Api.Common.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyDepartureAndRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_type", "comment_added,family_created,family_plan_created,monthly_recap_generated,plan_completed,plan_updated,vote_cast")
                .Annotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .Annotation("Npgsql:Enum:family_join_code_status", "active,capacity_reached,expired,revoked")
                .Annotation("Npgsql:Enum:family_join_request_status", "approved,cancelled,expired,pending,rejected,withdrawn")
                .Annotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .Annotation("Npgsql:Enum:family_member_status", "active,removed")
                .Annotation("Npgsql:Enum:family_recovery_code_status", "active,used")
                .Annotation("Npgsql:Enum:notification_type", "comment_added,family_join_request_approved,family_join_request_created,family_join_request_rejected,family_plan_created,milestone_completed,monthly_recap_ready,plan_updated,vote_cast")
                .Annotation("Npgsql:Enum:plan_status", "active,archived,completed")
                .Annotation("Npgsql:Enum:plan_type", "milestone,want_need")
                .Annotation("Npgsql:Enum:recap_period_type", "monthly")
                .Annotation("Npgsql:Enum:score_level", "high,low,medium")
                .Annotation("Npgsql:Enum:vote_value", "neutral,not_now,support")
                .Annotation("Npgsql:Enum:want_need_kind", "need,want")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:activity_type", "comment_added,family_created,family_plan_created,monthly_recap_generated,plan_completed,plan_updated,vote_cast")
                .OldAnnotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .OldAnnotation("Npgsql:Enum:family_join_code_status", "active,capacity_reached,expired,revoked")
                .OldAnnotation("Npgsql:Enum:family_join_request_status", "approved,expired,pending,rejected,withdrawn")
                .OldAnnotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .OldAnnotation("Npgsql:Enum:family_member_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:notification_type", "comment_added,family_join_request_approved,family_join_request_created,family_join_request_rejected,family_plan_created,milestone_completed,monthly_recap_ready,plan_updated,vote_cast")
                .OldAnnotation("Npgsql:Enum:plan_status", "active,archived,completed")
                .OldAnnotation("Npgsql:Enum:plan_type", "milestone,want_need")
                .OldAnnotation("Npgsql:Enum:recap_period_type", "monthly")
                .OldAnnotation("Npgsql:Enum:score_level", "high,low,medium")
                .OldAnnotation("Npgsql:Enum:vote_value", "neutral,not_now,support")
                .OldAnnotation("Npgsql:Enum:want_need_kind", "need,want")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "family_recovery_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creator_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<FamilyRecoveryCodeStatus>(type: "family_recovery_code_status", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    purge_claimed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_recovery_codes", x => x.id);
                    table.CheckConstraint("ck_family_recovery_codes_expiry", "expires_at > created_at");
                    table.ForeignKey(
                        name: "FK_family_recovery_codes_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_recovery_codes_users_creator_user_id",
                        column: x => x.creator_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_family_recovery_codes_active_expiry_claim",
                table: "family_recovery_codes",
                columns: new[] { "status", "expires_at", "purge_claimed_at" },
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "ix_family_recovery_codes_creator_user_id",
                table: "family_recovery_codes",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_family_recovery_codes_active_family_id",
                table: "family_recovery_codes",
                column: "family_id",
                unique: true,
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "ux_family_recovery_codes_code_hash",
                table: "family_recovery_codes",
                column: "code_hash",
                unique: true);

            migrationBuilder.Sql("ALTER TABLE family_recovery_codes ENABLE ROW LEVEL SECURITY;");
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon') THEN
                        REVOKE ALL ON TABLE family_recovery_codes FROM anon;
                    END IF;
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN
                        REVOKE ALL ON TABLE family_recovery_codes FROM authenticated;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_recovery_codes");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_type", "comment_added,family_created,family_plan_created,monthly_recap_generated,plan_completed,plan_updated,vote_cast")
                .Annotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .Annotation("Npgsql:Enum:family_join_code_status", "active,capacity_reached,expired,revoked")
                .Annotation("Npgsql:Enum:family_join_request_status", "approved,expired,pending,rejected,withdrawn")
                .Annotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .Annotation("Npgsql:Enum:family_member_status", "active,removed")
                .Annotation("Npgsql:Enum:notification_type", "comment_added,family_join_request_approved,family_join_request_created,family_join_request_rejected,family_plan_created,milestone_completed,monthly_recap_ready,plan_updated,vote_cast")
                .Annotation("Npgsql:Enum:plan_status", "active,archived,completed")
                .Annotation("Npgsql:Enum:plan_type", "milestone,want_need")
                .Annotation("Npgsql:Enum:recap_period_type", "monthly")
                .Annotation("Npgsql:Enum:score_level", "high,low,medium")
                .Annotation("Npgsql:Enum:vote_value", "neutral,not_now,support")
                .Annotation("Npgsql:Enum:want_need_kind", "need,want")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:activity_type", "comment_added,family_created,family_plan_created,monthly_recap_generated,plan_completed,plan_updated,vote_cast")
                .OldAnnotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .OldAnnotation("Npgsql:Enum:family_join_code_status", "active,capacity_reached,expired,revoked")
                .OldAnnotation("Npgsql:Enum:family_join_request_status", "approved,cancelled,expired,pending,rejected,withdrawn")
                .OldAnnotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .OldAnnotation("Npgsql:Enum:family_member_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:family_recovery_code_status", "active,used")
                .OldAnnotation("Npgsql:Enum:notification_type", "comment_added,family_join_request_approved,family_join_request_created,family_join_request_rejected,family_plan_created,milestone_completed,monthly_recap_ready,plan_updated,vote_cast")
                .OldAnnotation("Npgsql:Enum:plan_status", "active,archived,completed")
                .OldAnnotation("Npgsql:Enum:plan_type", "milestone,want_need")
                .OldAnnotation("Npgsql:Enum:recap_period_type", "monthly")
                .OldAnnotation("Npgsql:Enum:score_level", "high,low,medium")
                .OldAnnotation("Npgsql:Enum:vote_value", "neutral,not_now,support")
                .OldAnnotation("Npgsql:Enum:want_need_kind", "need,want")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }
    }
}
