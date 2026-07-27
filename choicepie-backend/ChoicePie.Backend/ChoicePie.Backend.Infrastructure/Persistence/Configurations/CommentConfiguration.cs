using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class CommentConfiguration : AuditableEntityConfiguration<Comment>
{
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.QuizId).IsRequired();
        builder.Property(c => c.Text).IsRequired().HasMaxLength(1000);

        builder.HasIndex(c => c.QuizId);
    }
}
