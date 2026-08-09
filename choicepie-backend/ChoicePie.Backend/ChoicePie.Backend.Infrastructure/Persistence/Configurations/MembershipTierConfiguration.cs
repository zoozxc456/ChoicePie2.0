using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class MembershipTierConfiguration : AuditableEntityConfiguration<MembershipTier>
{
    public override void Configure(EntityTypeBuilder<MembershipTier> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
        builder.Property(t => t.DailyGenerationLimit).IsRequired();
        builder.Property(t => t.DailyTokenBudget).IsRequired();
        builder.Property(t => t.IsDefault).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}
