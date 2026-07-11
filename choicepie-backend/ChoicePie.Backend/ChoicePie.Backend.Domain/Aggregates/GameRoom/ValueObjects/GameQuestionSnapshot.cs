using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

public sealed record GameQuestionSnapshot(
    Guid QuestionId,
    string Text,
    IReadOnlyList<string> Options,
    int AnswerIndex,
    string Explanation) : ValueObject;
