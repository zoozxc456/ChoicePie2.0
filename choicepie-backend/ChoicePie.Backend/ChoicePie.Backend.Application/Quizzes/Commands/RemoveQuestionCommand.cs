using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record RemoveQuestionCommand(Guid QuizId, Guid QuestionId) : IRequest<QuizDto>;
