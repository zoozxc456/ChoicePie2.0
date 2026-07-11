using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class QuizAttemptConfiguration : AuditableEntityConfiguration<QuizAttemptAggregate>
{
    public override void Configure(EntityTypeBuilder<QuizAttemptAggregate> builder)
    {
        base.Configure(builder);

        var guidListComparer = new ValueComparer<IReadOnlyList<Guid>>(
            (a, b) => (a ?? Array.Empty<Guid>()).SequenceEqual(b ?? Array.Empty<Guid>()),
            list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            list => list.ToList());

        builder.HasIndex(a => new { a.QuizId, a.MemberId });

        builder.Property(a => a.Status)
            .HasConversion(new EnumerationValueConverter<AttemptStatus>())
            .IsRequired();

        builder.Property(a => a.ExpectedQuestionIds)
            .HasColumnType("uuid[]")
            .Metadata.SetValueComparer(guidListComparer);

        builder.OwnsOne(a => a.Result, result =>
        {
            result.Property(r => r.Value).HasColumnName("Score");
        });

        builder.OwnsMany(a => a.Answers, answer => { answer.WithOwner().HasForeignKey("QuizAttemptId"); });
    }
}
