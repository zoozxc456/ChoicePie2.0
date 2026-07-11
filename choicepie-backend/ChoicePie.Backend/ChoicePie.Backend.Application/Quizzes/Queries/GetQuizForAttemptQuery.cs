using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed record GetQuizForAttemptQuery(Guid Id) : IRequest<QuizForAttemptDto>;
