using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Commands;

public sealed record CompleteQuizAttemptCommand(Guid AttemptId) : IRequest<QuizAttemptResultDto>;
