using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditableLoginMethodPlayerQuizAttemptAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "quiz_attempt_answer",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "creator_id",
                table: "quiz_attempt_answer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "quiz_attempt_answer",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleter_id",
                table: "quiz_attempt_answer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_modifer_id",
                table: "quiz_attempt_answer",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified_at",
                table: "quiz_attempt_answer",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "login_method",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "creator_id",
                table: "login_method",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "login_method",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "deleter_id",
                table: "login_method",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_modifer_id",
                table: "login_method",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified_at",
                table: "login_method",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                table: "quiz_attempt_answer");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "quiz_attempt_answer");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "quiz_attempt_answer");

            migrationBuilder.DropColumn(
                name: "deleter_id",
                table: "quiz_attempt_answer");

            migrationBuilder.DropColumn(
                name: "last_modifer_id",
                table: "quiz_attempt_answer");

            migrationBuilder.DropColumn(
                name: "last_modified_at",
                table: "quiz_attempt_answer");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "login_method");

            migrationBuilder.DropColumn(
                name: "creator_id",
                table: "login_method");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "login_method");

            migrationBuilder.DropColumn(
                name: "deleter_id",
                table: "login_method");

            migrationBuilder.DropColumn(
                name: "last_modifer_id",
                table: "login_method");

            migrationBuilder.DropColumn(
                name: "last_modified_at",
                table: "login_method");
        }
    }
}
