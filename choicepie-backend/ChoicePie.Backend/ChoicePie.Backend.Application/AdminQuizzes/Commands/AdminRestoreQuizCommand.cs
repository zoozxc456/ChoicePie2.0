using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Commands;

public sealed record AdminRestoreQuizCommand(Guid QuizId) : IRequest;
