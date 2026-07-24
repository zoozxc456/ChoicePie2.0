using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Queries;

public sealed record ListQuizAttemptHistoryQuery(Guid QuizId) : IRequest<IReadOnlyList<QuizAttemptHistoryItemDto>>;
