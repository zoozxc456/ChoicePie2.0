using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionPlayerMemberAndQuizSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "member_id",
                table: "game_session_player_result",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cover_emoji",
                table: "game_session",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cover_gradient",
                table: "game_session",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "quiz_id",
                table: "game_session",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "quiz_title",
                table: "game_session",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_player_result_member_id",
                table: "game_session_player_result",
                column: "member_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_game_session_player_result_member_id",
                table: "game_session_player_result");

            migrationBuilder.DropColumn(
                name: "member_id",
                table: "game_session_player_result");

            migrationBuilder.DropColumn(
                name: "cover_emoji",
                table: "game_session");

            migrationBuilder.DropColumn(
                name: "cover_gradient",
                table: "game_session");

            migrationBuilder.DropColumn(
                name: "quiz_id",
                table: "game_session");

            migrationBuilder.DropColumn(
                name: "quiz_title",
                table: "game_session");
        }
    }
}
