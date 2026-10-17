using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
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

        // 同一位會員對同一份題庫最多一個進行中的作答（連點「開始挑戰」時只會建立一筆）。
        builder.HasIndex(a => new { a.QuizId, a.MemberId }, "ix_quiz_attempt_one_in_progress_per_member")
            .HasDatabaseName("ix_quiz_attempt_one_in_progress_per_member")
            .IsUnique()
            .HasFilter($"status = {AttemptStatus.InProgress.Id}");

        // 繳交與作答會改寫同一筆作答，連點「繳交」時只有一次會成立，挑戰次數不會重複計算。
        builder.UseXminConcurrencyToken();

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
