using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class CreatorFollowConfiguration : AuditableEntityConfiguration<CreatorFollow>
{
    public override void Configure(EntityTypeBuilder<CreatorFollow> builder)
    {
        base.Configure(builder);

        builder.Property(f => f.FollowedCreatorId).IsRequired();

        builder.HasIndex(f => new { f.CreatorId, f.FollowedCreatorId }).IsUnique();
    }
}
