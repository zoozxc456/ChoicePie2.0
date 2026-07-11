using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class AuthAccountConfiguration : AuditableEntityConfiguration<AuthAccount>
{
    public override void Configure(EntityTypeBuilder<AuthAccount> builder)
    {
        base.Configure(builder);

        builder.Property(a => a.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .IsRequired();
        builder.HasIndex(a => a.Email).IsUnique();

        builder.Property(a => a.MemberId).IsRequired();

        builder.OwnsMany(a => a.LoginMethods, loginMethod =>
        {
            loginMethod.WithOwner().HasForeignKey("AuthAccountId");
            loginMethod.Property(m => m.Provider)
                .HasConversion(new EnumerationValueConverter<LoginProvider>())
                .IsRequired();
        });
    }
}
