using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizTakedownAndMemberSuspension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "takedown_at",
                table: "quiz",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "takedown_by",
                table: "quiz",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "takedown_reason",
                table: "quiz",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_suspended",
                table: "member",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "suspended_reason",
                table: "member",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "suspended_until",
                table: "member",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "takedown_at",
                table: "quiz");

            migrationBuilder.DropColumn(
                name: "takedown_by",
                table: "quiz");

            migrationBuilder.DropColumn(
                name: "takedown_reason",
                table: "quiz");

            migrationBuilder.DropColumn(
                name: "is_suspended",
                table: "member");

            migrationBuilder.DropColumn(
                name: "suspended_reason",
                table: "member");

            migrationBuilder.DropColumn(
                name: "suspended_until",
                table: "member");
        }
    }
}
