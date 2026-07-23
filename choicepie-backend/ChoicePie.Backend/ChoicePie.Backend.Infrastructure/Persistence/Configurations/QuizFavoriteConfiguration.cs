using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class QuizFavoriteConfiguration : AuditableEntityConfiguration<QuizFavorite>
{
    public override void Configure(EntityTypeBuilder<QuizFavorite> builder)
    {
        base.Configure(builder);

        builder.Property(f => f.QuizId).IsRequired();

        builder.HasIndex(f => new { f.CreatorId, f.QuizId }).IsUnique();
    }
}
