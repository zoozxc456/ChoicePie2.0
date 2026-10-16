using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class UpdateQuizCoverCommand : IRequest<QuizDto>
{
    public required Guid Id { get; init; }

    public string? CoverImageUrl { get; init; }

    [Required] public required string CoverEmoji { get; init; }

    [Required] public required string CoverGradient { get; init; }
}
