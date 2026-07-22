using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;

public sealed class Question : AuditableEntity<Guid>
{
    public const int RequiredOptionCount = ChoiceSet.RequiredOptionCount;

    public string Text { get; private set; } = null!;
    public ChoiceSet Choices { get; private set; } = null!;
    public string Explanation { get; private set; } = null!;

    public IReadOnlyList<string> Options => Choices.Options;
    public int AnswerIndex => Choices.AnswerIndex;

    private Question()
    {
    }

    public static Question Create(string text, IReadOnlyList<string> options, int answerIndex, string explanation)
    {
        ValidateText(text);

        return new Question
        {
            Id = Guid.NewGuid(),
            Text = text,
            Choices = ChoiceSet.Create(options, answerIndex),
            Explanation = explanation
        };
    }

    public void Update(string text, IReadOnlyList<string> options, int answerIndex, string explanation)
    {
        ValidateText(text);

        Text = text;
        Choices = ChoiceSet.Create(options, answerIndex);
        Explanation = explanation;
    }

    private static void ValidateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidQuestionException("題目文字不能為空。");
        }
    }
}
