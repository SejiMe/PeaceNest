using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PeaceNest.Api.Common.Database.Entities;

#nullable disable

namespace PeaceNest.Api.Common.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyJoinCodesAndRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .OldAnnotation("Npgsql:Enum:family_member_status", "active,removed")
                .OldAnnotation("Npgsql:Enum:notification_type", "comment_added,family_plan_created,milestone_completed,monthly_recap_ready,plan_updated,vote_cast")
                .OldAnnotation("Npgsql:Enum:plan_status", "active,archived,completed")
                .OldAnnotation("Npgsql:Enum:plan_type", "milestone,want_need")
                .OldAnnotation("Npgsql:Enum:recap_period_type", "monthly")
                .OldAnnotation("Npgsql:Enum:score_level", "high,low,medium")
                .OldAnnotation("Npgsql:Enum:vote_value", "neutral,not_now,support")
                .OldAnnotation("Npgsql:Enum:want_need_kind", "need,want")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AddColumn<Guid>(
                name: "related_join_request_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "family_join_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    status = table.Column<FamilyJoinCodeStatus>(type: "family_join_code_status", nullable: false),
                    request_count = table.Column<int>(type: "integer", nullable: false),
                    max_requests = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    revoked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_join_codes", x => x.id);
                    table.CheckConstraint("ck_family_join_codes_max_requests", "max_requests > 0");
                    table.CheckConstraint("ck_family_join_codes_request_count", "request_count >= 0 AND request_count <= max_requests");
                    table.ForeignKey(
                        name: "FK_family_join_codes_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_join_codes_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_join_codes_users_revoked_by_user_id",
                        column: x => x.revoked_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "family_join_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    join_code_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<FamilyJoinRequestStatus>(type: "family_join_request_status", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    approved_role = table.Column<FamilyMemberRole>(type: "family_member_role", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_join_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_family_join_requests_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_join_requests_family_join_codes_join_code_id",
                        column: x => x.join_code_id,
                        principalTable: "family_join_codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_join_requests_users_requester_user_id",
                        column: x => x.requester_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_join_requests_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_related_join_request_id",
                table: "notifications",
                column: "related_join_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_join_codes_created_by_user_id",
                table: "family_join_codes",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_join_codes_revoked_by_user_id",
                table: "family_join_codes",
                column: "revoked_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_family_join_codes_active_family",
                table: "family_join_codes",
                column: "family_id",
                unique: true,
                filter: "status = 'active'");

            migrationBuilder.CreateIndex(
                name: "ux_family_join_codes_code_hash",
                table: "family_join_codes",
                column: "code_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_join_requests_family_status_created",
                table: "family_join_requests",
                columns: new[] { "family_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_family_join_requests_join_code_id",
                table: "family_join_requests",
                column: "join_code_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_join_requests_requester_status_created",
                table: "family_join_requests",
                columns: new[] { "requester_user_id", "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_family_join_requests_reviewed_by_user_id",
                table: "family_join_requests",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_family_join_requests_pending_family_requester",
                table: "family_join_requests",
                columns: new[] { "family_id", "requester_user_id" },
                unique: true,
                filter: "status = 'pending'");

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_family_join_requests_related_join_request_id",
                table: "notifications",
                column: "related_join_request_id",
                principalTable: "family_join_requests",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                ALTER TABLE family_join_codes ENABLE ROW LEVEL SECURITY;
                ALTER TABLE family_join_requests ENABLE ROW LEVEL SECURITY;
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon') THEN
                        REVOKE ALL ON TABLE family_join_codes, family_join_requests FROM anon;
                    END IF;
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN
                        REVOKE ALL ON TABLE family_join_codes, family_join_requests FROM authenticated;
                    END IF;
                END
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_notifications_family_join_requests_related_join_request_id",
                table: "notifications");

            migrationBuilder.DropTable(
                name: "family_join_requests");

            migrationBuilder.DropTable(
                name: "family_join_codes");

            migrationBuilder.DropIndex(
                name: "ix_notifications_related_join_request_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "related_join_request_id",
                table: "notifications");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:activity_type", "comment_added,family_created,family_plan_created,monthly_recap_generated,plan_completed,plan_updated,vote_cast")
                .Annotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .Annotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .Annotation("Npgsql:Enum:family_member_status", "active,removed")
                .Annotation("Npgsql:Enum:notification_type", "comment_added,family_plan_created,milestone_completed,monthly_recap_ready,plan_updated,vote_cast")
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
        }
    }
}
