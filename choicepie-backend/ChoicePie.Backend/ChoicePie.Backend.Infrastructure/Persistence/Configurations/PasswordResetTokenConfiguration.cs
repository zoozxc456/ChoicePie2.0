using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordResetTokenAggregate = ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.PasswordResetToken;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration : AuditableEntityConfiguration<PasswordResetTokenAggregate>
{
    public override void Configure(EntityTypeBuilder<PasswordResetTokenAggregate> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.TokenHash).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        // 一次性 token：同一顆 token 同時被使用兩次時，只有先存檔的成功。
        builder.UseXminConcurrencyToken();
        builder.HasIndex(t => t.AuthAccountId);
    }
}
