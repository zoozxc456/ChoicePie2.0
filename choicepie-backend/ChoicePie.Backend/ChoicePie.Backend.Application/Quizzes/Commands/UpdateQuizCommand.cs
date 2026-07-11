using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class UpdateQuizCommand : IRequest<QuizDto>
{
    public required Guid Id { get; init; }

    [Required] [MaxLength(200)] public required string Title { get; init; }

    public string? Description { get; init; }

    public IReadOnlyList<string> Tags { get; init; } = [];
}
