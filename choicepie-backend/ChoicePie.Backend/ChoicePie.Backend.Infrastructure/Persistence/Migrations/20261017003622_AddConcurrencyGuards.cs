using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyGuards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "refresh_token",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "quiz_attempt",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "password_reset_token",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "email_verification_token",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            // 建唯一索引前先清掉並行漏洞已經造成的重複資料，否則索引建不起來。
            // 重複的待處理檢舉只保留最早的一筆；重複的進行中作答只保留最新的一筆（答案由 FK cascade 一併刪除）。
            migrationBuilder.Sql("""
                DELETE FROM quiz_report r
                USING quiz_report keep
                WHERE r.status = 1 AND keep.status = 1
                  AND r.quiz_id = keep.quiz_id AND r.creator_id = keep.creator_id
                  AND (r.created_at, r.id) > (keep.created_at, keep.id);

                DELETE FROM quiz_attempt a
                USING quiz_attempt keep
                WHERE a.status = 1 AND keep.status = 1
                  AND a.quiz_id = keep.quiz_id AND a.member_id = keep.member_id
                  AND (a.created_at, a.id) < (keep.created_at, keep.id);
                """);

            migrationBuilder.CreateIndex(
                name: "ix_quiz_report_one_pending_per_reporter",
                table: "quiz_report",
                columns: new[] { "quiz_id", "creator_id" },
                unique: true,
                filter: "status = 1");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempt_one_in_progress_per_member",
                table: "quiz_attempt",
                columns: new[] { "quiz_id", "member_id" },
                unique: true,
                filter: "status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_quiz_report_one_pending_per_reporter",
                table: "quiz_report");

            migrationBuilder.DropIndex(
                name: "ix_quiz_attempt_one_in_progress_per_member",
                table: "quiz_attempt");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "refresh_token");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "quiz_attempt");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "password_reset_token");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "email_verification_token");
        }
    }
}
