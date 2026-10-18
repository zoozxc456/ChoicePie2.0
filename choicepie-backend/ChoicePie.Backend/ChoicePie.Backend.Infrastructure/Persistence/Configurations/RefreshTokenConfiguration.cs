using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : AuditableEntityConfiguration<RefreshTokenAggregate>
{
    public override void Configure(EntityTypeBuilder<RefreshTokenAggregate> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.TokenHash).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        // 一次性 token：同一顆 token 同時被輪替/撤銷兩次時，只有先存檔的成功。
        builder.UseXminConcurrencyToken();
        builder.HasIndex(t => new { t.OwnerId, t.OwnerType });

        builder.Property(t => t.OwnerType)
            .HasConversion(new EnumerationValueConverter<RefreshTokenOwnerType>())
            .IsRequired();
    }
}
