using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record QuestionForAttemptDto(Guid Id, string Text, IReadOnlyList<string> Options)
{
    public static QuestionForAttemptDto FromDomain(Question question) =>
        new(question.Id, question.Text, question.Options);
}
