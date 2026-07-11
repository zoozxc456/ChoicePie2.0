using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;

public sealed record GameSessionQuestion(
    Guid QuestionId,
    string Text,
    IReadOnlyList<string> Options,
    int AnswerIndex,
    string Explanation) : ValueObject;
