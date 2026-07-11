using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Entities;

public sealed class QuizAttemptAnswer : AuditableEntity<Guid>
{
    public Guid QuestionId { get; private set; }
    public int SelectedOptionIndex { get; private set; }
    public DateTime AnsweredAt { get; private set; }

    private QuizAttemptAnswer()
    {
    }

    public static QuizAttemptAnswer Create(Guid questionId, int selectedOptionIndex, DateTime answeredAt) => new()
    {
        Id = Guid.NewGuid(),
        QuestionId = questionId,
        SelectedOptionIndex = selectedOptionIndex,
        AnsweredAt = answeredAt
    };
}
