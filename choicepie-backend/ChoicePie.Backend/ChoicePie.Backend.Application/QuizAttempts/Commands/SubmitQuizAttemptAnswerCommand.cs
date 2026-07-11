using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Commands;

public sealed record SubmitQuizAttemptAnswerCommand(Guid AttemptId, Guid QuestionId, int SelectedOptionIndex) : IRequest;
