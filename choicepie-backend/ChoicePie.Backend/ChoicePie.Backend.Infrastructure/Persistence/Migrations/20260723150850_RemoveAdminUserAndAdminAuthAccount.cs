using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminUserAndAdminAuthAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_login_method");

            migrationBuilder.DropTable(
                name: "admin_user");

            migrationBuilder.DropTable(
                name: "admin_auth_account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_auth_account",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    last_modifer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_auth_account", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_user",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modifer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admin_login_method",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    admin_auth_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleter_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modifer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Provider = table.Column<int>(type: "integer", nullable: true),
                    ProviderUserId = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    Salt = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_login_method", x => x.id);
                    table.ForeignKey(
                        name: "fk_admin_login_method_admin_auth_account_admin_auth_account_id",
                        column: x => x.admin_auth_account_id,
                        principalTable: "admin_auth_account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_auth_account_email",
                table: "admin_auth_account",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_admin_login_method_admin_auth_account_id",
                table: "admin_login_method",
                column: "admin_auth_account_id");
        }
    }
}
