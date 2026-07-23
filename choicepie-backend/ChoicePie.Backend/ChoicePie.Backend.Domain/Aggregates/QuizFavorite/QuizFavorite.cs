using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.QuizFavorite;

public sealed class QuizFavorite : AggregateRoot<Guid>
{
    public Guid QuizId { get; private set; }
    public Guid UserId => CreatorId!.Value;

    private QuizFavorite()
    {
    }

    public static QuizFavorite Create(Guid quizId, Guid userId)
    {
        var favorite = new QuizFavorite
        {
            Id = Guid.NewGuid(),
            QuizId = quizId
        };

        favorite.SetCreated(userId);

        return favorite;
    }
}
