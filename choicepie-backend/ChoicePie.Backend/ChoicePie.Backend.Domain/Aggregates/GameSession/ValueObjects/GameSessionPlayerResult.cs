using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;

public sealed record GameSessionPlayerResult(
    Guid PlayerId,
    string Nickname,
    Guid? MemberId,
    int FinalScore,
    int Rank,
    IReadOnlyList<GameSessionAnswerLogEntry> Answers) : ValueObject;
