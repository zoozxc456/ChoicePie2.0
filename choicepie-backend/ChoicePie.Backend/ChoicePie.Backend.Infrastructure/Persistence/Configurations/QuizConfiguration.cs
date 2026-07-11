using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class QuizConfiguration : AuditableEntityConfiguration<Quiz>
{
    public override void Configure(EntityTypeBuilder<Quiz> builder)
    {
        base.Configure(builder);

        var stringListComparer = new ValueComparer<IReadOnlyList<string>>(
            (a, b) => (a ?? Array.Empty<string>()).SequenceEqual(b ?? Array.Empty<string>()),
            list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            list => list.ToList());

        builder.Property(q => q.Title).IsRequired().HasMaxLength(200);

        builder.Property(q => q.Difficulty)
            .HasConversion(new EnumerationValueConverter<Difficulty>())
            .IsRequired();

        builder.Property(q => q.Status)
            .HasConversion(new EnumerationValueConverter<QuizStatus>())
            .IsRequired();

        builder.Property(q => q.Tags)
            .HasColumnType("text[]")
            .Metadata.SetValueComparer(stringListComparer);

        builder.OwnsMany(q => q.Questions, question =>
        {
            question.WithOwner().HasForeignKey("QuizId");

            question.Property(q => q.Options)
                .HasColumnType("text[]")
                .Metadata.SetValueComparer(stringListComparer);
        });
    }
}
