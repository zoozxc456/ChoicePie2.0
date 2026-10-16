using ChoicePie.Backend.Domain.Aggregates.AiUsageLog;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class AiUsageLogConfiguration : AuditableEntityConfiguration<AiUsageLog>
{
    public override void Configure(EntityTypeBuilder<AiUsageLog> builder)
    {
        base.Configure(builder);

        builder.Property(l => l.MemberId).IsRequired();
        builder.Property(l => l.Provider).HasMaxLength(50).IsRequired();
        builder.Property(l => l.Model).HasMaxLength(100).IsRequired();
        builder.Property(l => l.TokensUsed).IsRequired();

        builder.HasIndex(l => l.MemberId);
        builder.HasIndex(l => l.CreatedAt);
    }
}
