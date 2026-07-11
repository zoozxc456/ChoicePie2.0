using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class MemberConfiguration : AuditableEntityConfiguration<Member>
{
    public override void Configure(EntityTypeBuilder<Member> builder)
    {
        base.Configure(builder);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(20);
    }
}
