using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Commands;

public sealed class GenerateQuizQuestionsCommand : IRequest<GenerateQuestionsResultDto>
{
    [Required] [MinLength(30)] [MaxLength(5000)] public required string Content { get; init; }

    public required int QuestionCount { get; init; }

    [Required] public required string Difficulty { get; init; }
}
