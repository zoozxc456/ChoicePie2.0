using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizFavorite.Specifications;

public sealed class QuizFavoriteByUserAndQuizSpecification(Guid userId, Guid quizId)
    : Specification<QuizFavorite>(f => f.UserId == userId && f.QuizId == quizId);
