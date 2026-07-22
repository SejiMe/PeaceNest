using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeaceNest.Api.Common.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileCurrencyAndInvitationCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "users",
                type: "character(2)",
                fixedLength: true,
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "onboarding_completed_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invitation_code_hash",
                table: "family_invitations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_currency",
                table: "families",
                type: "char(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql("UPDATE families SET preferred_currency = 'PHP' WHERE preferred_currency IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "preferred_currency",
                table: "families",
                type: "char(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_family_invitations_invitation_code_hash",
                table: "family_invitations",
                column: "invitation_code_hash",
                unique: true,
                filter: "invitation_code_hash IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_family_invitations_invitation_code_hash",
                table: "family_invitations");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "users");

            migrationBuilder.DropColumn(
                name: "onboarding_completed_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "invitation_code_hash",
                table: "family_invitations");

            migrationBuilder.DropColumn(
                name: "preferred_currency",
                table: "families");
        }
    }
}
