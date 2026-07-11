using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

public sealed record QuizCover : ValueObject
{
    public string Emoji { get; }
    public string Gradient { get; }

    private QuizCover(string emoji, string gradient)
    {
        Emoji = emoji;
        Gradient = gradient;
    }

    public static QuizCover Create(string emoji, string gradient)
    {
        return new QuizCover(emoji, gradient);
    }
}
