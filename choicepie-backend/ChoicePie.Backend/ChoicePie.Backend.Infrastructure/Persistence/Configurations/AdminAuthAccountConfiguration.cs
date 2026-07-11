using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class AdminAuthAccountConfiguration : AuditableEntityConfiguration<AdminAuthAccount>
{
    public override void Configure(EntityTypeBuilder<AdminAuthAccount> builder)
    {
        base.Configure(builder);

        builder.Property(a => a.Email)
            .HasConversion(e => e.Value, v => Email.Create(v))
            .IsRequired();
        builder.HasIndex(a => a.Email).IsUnique();

        builder.Property(a => a.AdminUserId).IsRequired();

        builder.OwnsMany(a => a.LoginMethods, loginMethod =>
        {
            loginMethod.WithOwner().HasForeignKey("AdminAuthAccountId");
            loginMethod.Property(m => m.CreatedAt).IsRequired();
            loginMethod.Property(m => m.LastModifiedAt).IsRequired();
            loginMethod.Property(m => m.Provider)
                .HasConversion(new EnumerationValueConverter<AdminLoginProvider>())
                .IsRequired();

            loginMethod.OwnsOne(m => m.Password, password =>
            {
                password.Property(p => p.Hash).HasColumnName("PasswordHash");
                password.Property(p => p.Salt).HasColumnName("Salt");
            });
        });
    }
}
