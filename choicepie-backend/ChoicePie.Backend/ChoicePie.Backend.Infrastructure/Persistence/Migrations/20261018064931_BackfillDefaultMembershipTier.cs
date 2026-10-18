using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoicePie.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BackfillDefaultMembershipTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddMembershipTier 之後註冊的會員沒有被指派方案，補上預設方案；沒有預設方案時不動。
            migrationBuilder.Sql("""
                UPDATE member
                SET tier_id = (
                    SELECT id FROM membership_tier
                    WHERE is_default AND deleted_at IS NULL
                    ORDER BY created_at
                    LIMIT 1)
                WHERE tier_id IS NULL
                  AND EXISTS (SELECT 1 FROM membership_tier WHERE is_default AND deleted_at IS NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 無法分辨哪些會員是這次補上的，不還原。
        }
    }
}
