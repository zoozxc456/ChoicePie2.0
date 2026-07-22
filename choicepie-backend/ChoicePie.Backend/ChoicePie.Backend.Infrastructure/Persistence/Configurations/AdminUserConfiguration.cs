using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class AdminUserConfiguration : AuditableEntityConfiguration<AdminUser>
{
    public override void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        base.Configure(builder);

        builder.Property(a => a.Name).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Role)
            .HasConversion(new EnumerationValueConverter<AdminRole>())
            .IsRequired();
    }
}
