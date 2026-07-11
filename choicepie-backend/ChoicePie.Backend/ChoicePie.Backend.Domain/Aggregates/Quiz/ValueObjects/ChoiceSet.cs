using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

public sealed record ChoiceSet : ValueObject
{
    public const int RequiredOptionCount = 4;

    public IReadOnlyList<string> Options { get; }
    public int AnswerIndex { get; }

    private ChoiceSet(IReadOnlyList<string> options, int answerIndex)
    {
        Options = options;
        AnswerIndex = answerIndex;
    }

    public static ChoiceSet Create(IReadOnlyList<string> options, int answerIndex)
    {
        if (options.Count != RequiredOptionCount || options.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidQuestionException($"每題必須恰好有 {RequiredOptionCount} 個非空選項。");
        }

        if (answerIndex < 0 || answerIndex >= options.Count)
        {
            throw new InvalidQuestionException("答案索引超出選項範圍。");
        }

        return new ChoiceSet(options, answerIndex);
    }
}
