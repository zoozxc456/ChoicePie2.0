using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EmailVerificationTokenAggregate = ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.EmailVerificationToken;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class EmailVerificationTokenConfiguration : AuditableEntityConfiguration<EmailVerificationTokenAggregate>
{
    public override void Configure(EntityTypeBuilder<EmailVerificationTokenAggregate> builder)
    {
        base.Configure(builder);

        builder.Property(t => t.TokenHash).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.AuthAccountId);
    }
}
