using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Entities;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Events;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.QuizAttempt;

public sealed class QuizAttempt : AggregateRoot<Guid>
{
    private readonly List<Guid> _expectedQuestionIds = [];
    private readonly List<QuizAttemptAnswer> _answers = [];

    public Guid QuizId { get; private set; }
    public Guid MemberId { get; private set; }
    public AttemptStatus Status { get; private set; } = null!;
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public AttemptScore? Result { get; private set; }

    [NotMapped]
    public decimal? Score => Result?.Value;

    [NotMapped]
    public bool? Passed => Result?.Passed;

    public IReadOnlyList<Guid> ExpectedQuestionIds => _expectedQuestionIds.AsReadOnly();
    public IReadOnlyList<QuizAttemptAnswer> Answers => _answers.AsReadOnly();

    private QuizAttempt()
    {
    }

    public static QuizAttempt Start(Guid quizId, Guid memberId, IReadOnlyList<Guid> questionIds, DateTime utcNow)
    {
        if (questionIds.Count == 0)
        {
            throw new InvalidQuizAttemptException("題庫沒有題目，無法開始作答。");
        }

        var attempt = new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            MemberId = memberId,
            Status = AttemptStatus.InProgress,
            StartedAt = utcNow
        };
        attempt._expectedQuestionIds.AddRange(questionIds);

        attempt.SetCreated(memberId);

        return attempt;
    }

    public void SubmitAnswer(Guid questionId, int selectedOptionIndex, DateTime utcNow)
    {
        if (Status != AttemptStatus.InProgress)
        {
            throw new InvalidQuizAttemptStateException(Id, "作答已結束，無法再提交答案。");
        }

        if (!_expectedQuestionIds.Contains(questionId))
        {
            throw new InvalidQuizAttemptException("此題目不屬於本次作答的題庫。");
        }

        _answers.RemoveAll(a => a.QuestionId == questionId);
        _answers.Add(QuizAttemptAnswer.Create(questionId, selectedOptionIndex, utcNow));

        Touch();
    }

    public void Complete(IReadOnlyDictionary<Guid, int> correctAnswerIndexByQuestionId, DateTime utcNow)
    {
        if (Status != AttemptStatus.InProgress)
        {
            throw new InvalidQuizAttemptStateException(Id, "作答已經結束，無法重複繳交。");
        }

        var correctCount = _answers.Count(a =>
            correctAnswerIndexByQuestionId.TryGetValue(a.QuestionId, out var correctIndex) &&
            correctIndex == a.SelectedOptionIndex);

        Result = AttemptScore.FromCorrectCount(correctCount, _expectedQuestionIds.Count);
        Status = AttemptStatus.Completed;
        CompletedAt = utcNow;
        Touch();

        AddDomainEvent(new QuizAttemptCompletedDomainEvent(Id, QuizId, MemberId, Result.Value, Result.Passed));
    }

    public void EnsureOwnedBy(Guid memberId)
    {
        if (MemberId != memberId)
        {
            throw new QuizAttemptForbiddenException(Id, memberId);
        }
    }
}
