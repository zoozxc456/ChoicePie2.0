using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record PublishQuizCommand(Guid Id) : IRequest<QuizDto>;
