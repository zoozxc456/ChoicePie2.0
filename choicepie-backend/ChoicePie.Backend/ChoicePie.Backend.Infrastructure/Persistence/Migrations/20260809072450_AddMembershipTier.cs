using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipTier : Migration
    {
        private static readonly Guid DefaultTierId = new("11111111-1111-1111-1111-111111111111");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "tier_id",
                table: "member",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "membership_tier",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    daily_generation_limit = table.Column<int>(type: "integer", nullable: false),
                    daily_token_budget = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modifer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_membership_tier", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_member_tier_id",
                table: "member",
                column: "tier_id");

            migrationBuilder.CreateIndex(
                name: "ix_membership_tier_name",
                table: "membership_tier",
                column: "name",
                unique: true);

            migrationBuilder.InsertData(
                table: "membership_tier",
                columns:
                [
                    "id", "name", "daily_generation_limit", "daily_token_budget", "is_default", "created_at", "last_modified_at"
                ],
                values: new object[] { DefaultTierId, "Free", 3, 10_000, true, DateTime.UtcNow, DateTime.UtcNow });

            migrationBuilder.Sql(
                $"UPDATE member SET tier_id = '{DefaultTierId}' WHERE tier_id IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "membership_tier");

            migrationBuilder.DropIndex(
                name: "ix_member_tier_id",
                table: "member");

            migrationBuilder.DropColumn(
                name: "tier_id",
                table: "member");
        }
    }
}
