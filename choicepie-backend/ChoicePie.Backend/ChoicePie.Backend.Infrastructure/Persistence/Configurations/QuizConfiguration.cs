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

        builder.Property(q => q.ShareCount).IsRequired();

        builder.Property(q => q.Difficulty)
            .HasConversion(new EnumerationValueConverter<Difficulty>())
            .IsRequired();

        builder.Property(q => q.Status)
            .HasConversion(new EnumerationValueConverter<QuizStatus>())
            .IsRequired();

        builder.Property(q => q.Tags)
            .HasColumnType("text[]")
            .Metadata.SetValueComparer(stringListComparer);

        builder.OwnsOne(q => q.Stats, stats =>
        {
            stats.Property(s => s.Count).HasColumnName("ChallengeCount").IsRequired();
            stats.Property(s => s.PassRate).HasColumnName("PassRate").IsRequired();
        });

        builder.OwnsOne(q => q.Cover, cover =>
        {
            cover.Property(c => c.Emoji).HasColumnName("CoverEmoji").IsRequired();
            cover.Property(c => c.Gradient).HasColumnName("CoverGradient").IsRequired();
        });

        builder.OwnsMany(q => q.Questions, question =>
        {
            question.WithOwner().HasForeignKey("QuizId");

            question.OwnsOne(q => q.Choices, choices =>
            {
                choices.Property(c => c.Options)
                    .HasColumnType("text[]")
                    .Metadata.SetValueComparer(stringListComparer);
                choices.Property(c => c.AnswerIndex).IsRequired();
            });
        });
    }
}
