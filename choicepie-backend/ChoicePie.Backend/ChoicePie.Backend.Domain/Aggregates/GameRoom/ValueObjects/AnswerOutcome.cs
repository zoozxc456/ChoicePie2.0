using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

public sealed record AnswerOutcome(int Score, bool IsCorrect, int CorrectAnswerIndex) : ValueObject;
