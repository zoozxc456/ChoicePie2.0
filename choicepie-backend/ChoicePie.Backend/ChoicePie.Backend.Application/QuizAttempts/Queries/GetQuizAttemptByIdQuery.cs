using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Queries;

public sealed record GetQuizAttemptByIdQuery(Guid Id) : IRequest<QuizAttemptResultDto>;
