using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Queries;

public sealed record GetQuizFavoriteStatusQuery(Guid QuizId) : IRequest<bool>;
