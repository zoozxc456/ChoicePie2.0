using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizCoverImageUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "quiz",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cover_image_url",
                table: "game_session",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            // 既有資料的 CoverGradient 是舊的 CSS linear-gradient 字串，QuizCover.Create 現在只接受
            // 6 個固定色鍵（primary/secondary/success/danger/warning/info），改寫成 secondary（深灰，
            // 與舊深藍紫漸層視覺上最接近）避免既有題庫在新的 fallback 邏輯下顯示異常。
            migrationBuilder.Sql("""
                UPDATE quiz SET "CoverGradient" = 'secondary' WHERE "CoverGradient" LIKE 'linear-gradient%';
                UPDATE game_session SET cover_gradient = 'secondary' WHERE cover_gradient LIKE 'linear-gradient%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "quiz");

            migrationBuilder.DropColumn(
                name: "cover_image_url",
                table: "game_session");
        }
    }
}
