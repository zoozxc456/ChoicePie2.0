using System.Text.Json;
using ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;

namespace ChoicePie.Backend.Infrastructure.Persistence.Configurations;

public sealed class GameSessionConfiguration : AuditableEntityConfiguration<GameSessionAggregate>
{
    public override void Configure(EntityTypeBuilder<GameSessionAggregate> builder)
    {
        base.Configure(builder);

        var stringListComparer = new ValueComparer<IReadOnlyList<string>>(
            (a, b) => (a ?? Array.Empty<string>()).SequenceEqual(b ?? Array.Empty<string>()),
            list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            list => list.ToList());

        // Answers 是「owned collection 內的 owned collection」，EF 的 owned-type 建構式綁定不支援兩層巢狀導覽，
        // 且這份逐題作答記錄只作為歷史複習資料讀取、不需要關聯式查詢，因此改以 jsonb 序列化儲存。
        var answerLogComparer = new ValueComparer<IReadOnlyList<GameSessionAnswerLogEntry>>(
            (a, b) => (a ?? Array.Empty<GameSessionAnswerLogEntry>()).SequenceEqual(b ?? Array.Empty<GameSessionAnswerLogEntry>()),
            list => list.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            list => list.ToList());

        builder.HasIndex(s => s.RoomCode);
        builder.HasIndex(s => s.HostUserId);

        builder.Property(s => s.RoomCode).IsRequired().HasMaxLength(6);

        builder.OwnsMany(s => s.Questions, question =>
        {
            question.WithOwner().HasForeignKey("GameSessionId");

            question.Property(q => q.Options)
                .HasColumnType("text[]")
                .Metadata.SetValueComparer(stringListComparer);
        });

        builder.OwnsMany(s => s.PlayerResults, result =>
        {
            result.WithOwner().HasForeignKey("GameSessionId");

            result.Property(r => r.Answers)
                .HasConversion(
                    answers => JsonSerializer.Serialize(answers, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<List<GameSessionAnswerLogEntry>>(json, (JsonSerializerOptions?)null) ?? new List<GameSessionAnswerLogEntry>())
                .HasColumnType("jsonb")
                .Metadata.SetValueComparer(answerLogComparer);
        });
    }
}
