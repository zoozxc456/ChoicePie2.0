using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;

namespace ChoicePie.Backend.Application.Quizzes.Dtos;

public sealed record QuestionStubDto(Guid Id)
{
    public static QuestionStubDto FromDomain(Question question) => new(question.Id);
}
