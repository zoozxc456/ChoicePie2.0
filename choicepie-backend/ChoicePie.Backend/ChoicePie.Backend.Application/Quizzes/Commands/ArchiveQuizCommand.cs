using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record ArchiveQuizCommand(Guid Id) : IRequest<QuizDto>;
