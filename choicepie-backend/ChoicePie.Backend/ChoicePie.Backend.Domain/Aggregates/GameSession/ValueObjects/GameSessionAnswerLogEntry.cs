using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;

public sealed record GameSessionAnswerLogEntry(
    int QuestionIndex,
    int SelectedOptionIndex,
    bool IsCorrect,
    int ScoreAwarded) : ValueObject;
