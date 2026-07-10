using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record DeleteQuizCommand(Guid Id) : IRequest;
