using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;

public sealed class Question : AuditableEntity<Guid>
{
    public const int RequiredOptionCount = 4;
    public string Text { get; private set; }
    public IReadOnlyList<string> Options { get; private set; }
    public int AnswerIndex { get; private set; }
    public string Explanation { get; private set; }

    private Question(Guid id, string text, IReadOnlyList<string> options, int answerIndex, string explanation)
    {
        Id = id;
        Text = text;
        Options = options;
        AnswerIndex = answerIndex;
        Explanation = explanation;
    }

    public static Question Create(string text, IReadOnlyList<string> options, int answerIndex, string explanation)
    {
        Validate(text, options, answerIndex);

        return new Question(Guid.NewGuid(), text, options, answerIndex, explanation);
    }

    public void Update(string text, IReadOnlyList<string> options, int answerIndex, string explanation)
    {
        Validate(text, options, answerIndex);

        Text = text;
        Options = options;
        AnswerIndex = answerIndex;
        Explanation = explanation;
    }

    private static void Validate(string text, IReadOnlyList<string> options, int answerIndex)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidQuestionException("題目文字不能為空。");
        }

        if (options.Count != RequiredOptionCount || options.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidQuestionException($"每題必須恰好有 {RequiredOptionCount} 個非空選項。");
        }

        if (answerIndex < 0 || answerIndex >= options.Count)
        {
            throw new InvalidQuestionException("答案索引超出選項範圍。");
        }
    }
}
