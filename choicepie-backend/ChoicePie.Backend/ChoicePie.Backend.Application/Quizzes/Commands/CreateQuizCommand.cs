using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed record CreateQuestionDto(string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation);

public sealed class CreateQuizCommand : IRequest<QuizDto>
{
    [Required] [MaxLength(200)] public required string Title { get; init; }

    public string? Description { get; init; }

    [Required] public required string CoverEmoji { get; init; }

    [Required] public required string CoverGradient { get; init; }

    [Required] public required string Difficulty { get; init; }

    public bool IsPublic { get; init; } = true;

    public IReadOnlyList<string> Tags { get; init; } = [];

    [Required] [MinLength(1)] public required IReadOnlyList<CreateQuestionDto> Questions { get; init; }
}
