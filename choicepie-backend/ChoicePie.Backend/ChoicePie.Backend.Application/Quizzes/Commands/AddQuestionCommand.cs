using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record AddQuestionCommand : IRequest<QuizDto>
{
    public required Guid QuizId { get; init; }

    [Required] public required string Text { get; init; }

    [Required] public required IReadOnlyList<string> Options { get; init; }

    public required int AnswerIndex { get; init; }

    [Required] public required string Explanation { get; init; }
}
