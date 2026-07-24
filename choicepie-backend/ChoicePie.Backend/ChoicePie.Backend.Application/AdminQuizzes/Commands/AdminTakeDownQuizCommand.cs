using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Commands;

public sealed record AdminTakeDownQuizCommand(Guid QuizId, string Reason) : IRequest;
