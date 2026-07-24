using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quiz_report",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quiz_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    resolution_note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_report", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_report_quiz_id",
                table: "quiz_report",
                column: "quiz_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_report_status",
                table: "quiz_report",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_report");
        }
    }
}
