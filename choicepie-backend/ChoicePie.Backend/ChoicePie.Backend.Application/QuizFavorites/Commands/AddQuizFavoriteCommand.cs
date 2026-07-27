using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Commands;

public sealed record AddQuizFavoriteCommand(Guid QuizId) : IRequest;
