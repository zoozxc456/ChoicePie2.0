using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed record GetRelatedQuizzesQuery(Guid QuizId, int Limit) : IRequest<IReadOnlyList<QuizSummaryDto>>;
