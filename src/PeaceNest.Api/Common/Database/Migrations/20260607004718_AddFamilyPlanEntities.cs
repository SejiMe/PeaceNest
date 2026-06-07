using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using PeaceNest.Api.Common.Database.Entities;

#nullable disable

namespace PeaceNest.Api.Common.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyPlanEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .OldAnnotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .OldAnnotation("Npgsql:Enum:family_member_status", "active,removed")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "plan_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icon = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    color = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_categories_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recaps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_type = table.Column<RecapPeriodType>(type: "recap_period_type", nullable: false),
                    period_start = table.Column<DateOnly>(type: "date", nullable: false),
                    period_end = table.Column<DateOnly>(type: "date", nullable: false),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    summary = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    stats = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    generated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recaps", x => x.id);
                    table.ForeignKey(
                        name: "FK_recaps_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recaps_users_generated_by_user_id",
                        column: x => x.generated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "family_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_type = table.Column<PlanType>(type: "plan_type", nullable: false),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<PlanStatus>(type: "plan_status", nullable: false),
                    priority_rank = table.Column<int>(type: "integer", nullable: true),
                    priority_score = table.Column<decimal>(type: "numeric(9,2)", precision: 9, scale: 2, nullable: false),
                    progress_percent = table.Column<int>(type: "integer", nullable: false),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_family_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_family_plans_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_plans_plan_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "plan_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_family_plans_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comments", x => x.id);
                    table.ForeignKey(
                        name: "FK_comments_comments_parent_comment_id",
                        column: x => x.parent_comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_comments_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "goal_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_steps", x => x.id);
                    table.ForeignKey(
                        name: "FK_goal_steps_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_goal_steps_users_completed_by_user_id",
                        column: x => x.completed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "memories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    caption = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    media_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    storage_provider = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    thumbnail_path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    taken_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_memories", x => x.id);
                    table.ForeignKey(
                        name: "FK_memories_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memories_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_memories_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "milestone_details",
                columns: table => new
                {
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    milestone_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    celebration_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reflection_prompt = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    include_in_recap = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_milestone_details", x => x.plan_id);
                    table.ForeignKey(
                        name: "FK_milestone_details_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_participants", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_participants_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_participants_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_votes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote_value = table.Column<VoteValue>(type: "vote_value", nullable: false),
                    priority_points = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_votes", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_votes_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_votes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "want_need_details",
                columns: table => new
                {
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<WantNeedKind>(type: "want_need_kind", nullable: false),
                    estimated_cost_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    estimated_cost_currency = table.Column<string>(type: "char(3)", maxLength: 3, nullable: true),
                    urgency_level = table.Column<ScoreLevel>(type: "score_level", nullable: false),
                    importance_level = table.Column<ScoreLevel>(type: "score_level", nullable: false),
                    emotional_value_level = table.Column<ScoreLevel>(type: "score_level", nullable: false),
                    desired_by_date = table.Column<DateOnly>(type: "date", nullable: true),
                    funding_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_want_need_details", x => x.plan_id);
                    table.ForeignKey(
                        name: "FK_want_need_details_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_type = table.Column<ActivityType>(type: "activity_type", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recap_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_activity_logs_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_logs_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_logs_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_logs_recaps_recap_id",
                        column: x => x.recap_id,
                        principalTable: "recaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_activity_logs_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    family_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<NotificationType>(type: "notification_type", nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    body = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    related_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_recap_id = table.Column<Guid>(type: "uuid", nullable: true),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_comments_related_comment_id",
                        column: x => x.related_comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_families_family_id",
                        column: x => x.family_id,
                        principalTable: "families",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_family_plans_related_plan_id",
                        column: x => x.related_plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_recaps_related_recap_id",
                        column: x => x.related_recap_id,
                        principalTable: "recaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notifications_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reaction_type = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_reactions_comments_comment_id",
                        column: x => x.comment_id,
                        principalTable: "comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reactions_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "recap_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recap_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    memory_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_type = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    title = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recap_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_recap_items_family_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "family_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recap_items_memories_memory_id",
                        column: x => x.memory_id,
                        principalTable: "memories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recap_items_recaps_recap_id",
                        column: x => x.recap_id,
                        principalTable: "recaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_actor_user_id",
                table: "activity_logs",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_comment_id",
                table: "activity_logs",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_family_id_created_at",
                table: "activity_logs",
                columns: new[] { "family_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_plan_id",
                table: "activity_logs",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_logs_recap_id",
                table: "activity_logs",
                column: "recap_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_active_plan_id_created_at",
                table: "comments",
                columns: new[] { "plan_id", "created_at" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_comments_author_user_id",
                table: "comments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_parent_comment_id",
                table: "comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_plans_active_priority_rank",
                table: "family_plans",
                columns: new[] { "family_id", "priority_rank" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_family_plans_category_id",
                table: "family_plans",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_plans_created_by_user_id",
                table: "family_plans",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_plans_family_id_plan_type_status",
                table: "family_plans",
                columns: new[] { "family_id", "plan_type", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_goal_steps_active_plan_id_sort_order",
                table: "goal_steps",
                columns: new[] { "plan_id", "sort_order" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_goal_steps_completed_by_user_id",
                table: "goal_steps",
                column: "completed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_memories_active_family_id_created_at",
                table: "memories",
                columns: new[] { "family_id", "created_at" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_memories_plan_id",
                table: "memories",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_memories_uploaded_by_user_id",
                table: "memories",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_active_family_created",
                table: "notifications",
                columns: new[] { "family_id", "created_at" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_active_recipient_read_created",
                table: "notifications",
                columns: new[] { "recipient_user_id", "read_at", "created_at" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_actor_user_id",
                table: "notifications",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_related_comment_id",
                table: "notifications",
                column: "related_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_related_plan_id",
                table: "notifications",
                column: "related_plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_related_recap_id",
                table: "notifications",
                column: "related_recap_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_categories_active_name",
                table: "plan_categories",
                columns: new[] { "family_id", "name" },
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_plan_categories_family_id_sort_order",
                table: "plan_categories",
                columns: new[] { "family_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_plan_participants_user_id",
                table: "plan_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_plan_participants_plan_id_user_id",
                table: "plan_participants",
                columns: new[] { "plan_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plan_votes_user_id",
                table: "plan_votes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_plan_votes_plan_id_user_id",
                table: "plan_votes",
                columns: new[] { "plan_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_reactions_user_id",
                table: "reactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_reactions_comment_user_type",
                table: "reactions",
                columns: new[] { "comment_id", "user_id", "reaction_type" },
                unique: true,
                filter: "comment_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_reactions_plan_user_type",
                table: "reactions",
                columns: new[] { "plan_id", "user_id", "reaction_type" },
                unique: true,
                filter: "plan_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_recap_items_memory_id",
                table: "recap_items",
                column: "memory_id");

            migrationBuilder.CreateIndex(
                name: "ix_recap_items_plan_id",
                table: "recap_items",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_recap_items_recap_id_sort_order",
                table: "recap_items",
                columns: new[] { "recap_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_recaps_generated_by_user_id",
                table: "recaps",
                column: "generated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_recaps_active_family_period",
                table: "recaps",
                columns: new[] { "family_id", "period_type", "period_start" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_logs");

            migrationBuilder.DropTable(
                name: "goal_steps");

            migrationBuilder.DropTable(
                name: "milestone_details");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "plan_participants");

            migrationBuilder.DropTable(
                name: "plan_votes");

            migrationBuilder.DropTable(
                name: "reactions");

            migrationBuilder.DropTable(
                name: "recap_items");

            migrationBuilder.DropTable(
                name: "want_need_details");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "memories");

            migrationBuilder.DropTable(
                name: "recaps");

            migrationBuilder.DropTable(
                name: "family_plans");

            migrationBuilder.DropTable(
                name: "plan_categories");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:family_invitation_status", "accepted,expired,pending,revoked")
                .Annotation("Npgsql:Enum:family_member_role", "adult_member,child_member,owner,parent_admin,viewer")
                .Annotation("Npgsql:Enum:family_member_status", "active,removed")
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
        }
    }
}
