using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record QuestionDto(Guid Id, string Text, IReadOnlyList<string> Options, int AnswerIndex, string Explanation)
{
    public static QuestionDto FromDomain(Question question) =>
        new(question.Id, question.Text, question.Options, question.AnswerIndex, question.Explanation);
}
