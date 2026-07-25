using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record UnarchiveQuizCommand(Guid Id) : IRequest<QuizDto>;
