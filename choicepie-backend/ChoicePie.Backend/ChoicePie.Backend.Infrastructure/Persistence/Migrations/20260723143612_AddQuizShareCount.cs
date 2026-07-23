using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizShareCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "share_count",
                table: "quiz",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "share_count",
                table: "quiz");
        }
    }
}
