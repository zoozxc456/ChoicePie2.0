using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz;

public sealed class Quiz : AggregateRoot<Guid>
{
    private readonly List<Question> _questions = [];
    private readonly List<string> _tags = [];

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public QuizCover Cover { get; private set; } = null!;
    public Difficulty Difficulty { get; private set; } = null!;
    public QuizStatus Status { get; private set; } = null!;

    // Reuses the inherited audit CreatorId (set by SetCreated below) rather than storing the
    // owner twice - the creator IS the owner for authorization purposes in this phase.
    [NotMapped] public Guid OwnerId => CreatorId!.Value;

    // Updated by RecordChallengeOutcome, called from a QuizAttemptCompletedDomainEvent handler.
    // EfUnitOfWork dispatches domain events after commit, outside any transaction, so that
    // handler is at-least-once delivery, not exactly-once - best effort, not deduplicated.
    public ChallengeStats Stats { get; private set; } = ChallengeStats.None;

    [NotMapped] public int ChallengeCount => Stats.Count;

    [NotMapped] public decimal PassRate => Stats.PassRate;

    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    // Fully derivable from data this aggregate already owns - must stay computed, not persisted,
    // or it becomes a second source of truth alongside Questions.
    [NotMapped] public int QuestionCount => _questions.Count;

    private Quiz()
    {
    }

    public static Quiz Create(
        Guid creatorId,
        string title,
        string? description,
        string coverEmoji,
        string coverGradient,
        Difficulty difficulty,
        IEnumerable<string> tags)
    {
        ValidateTitle(title);

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Cover = QuizCover.Create(coverEmoji, coverGradient),
            Difficulty = difficulty,
            Status = QuizStatus.Draft
        };
        quiz._tags.AddRange(TagList.Create(tags).Values);

        quiz.SetCreated(creatorId);
        quiz.AddDomainEvent(new QuizCreatedDomainEvent(quiz.Id, creatorId, quiz.Title));

        return quiz;
    }

    public new void Delete(Guid deleterId)
    {
        Status = QuizStatus.Deleted;
        base.Delete(deleterId);
    }

    public void EnsureModifiableBy(Guid userId)
    {
        if (OwnerId != userId)
        {
            throw new QuizForbiddenException(Id, userId);
        }
    }

    public void AddQuestion(Question question)
    {
        EnsureQuestionsEditable();

        _questions.Add(question);
        Touch();
    }

    public void RemoveQuestion(Guid questionId)
    {
        EnsureQuestionsEditable();

        _questions.RemoveAll(q => q.Id == questionId);
        Touch();
    }

    public void UpdateQuestion(Guid questionId, string text, IReadOnlyList<string> options, int answerIndex,
        string explanation)
    {
        EnsureQuestionsEditable();

        var question = _questions.SingleOrDefault(q => q.Id == questionId)
                       ?? throw new InvalidQuestionException("找不到指定的題目。");

        question.Update(text, options, answerIndex, explanation);
        Touch();
    }

    public void UpdateDetails(string title, string? description, IEnumerable<string> tags)
    {
        EnsureEditable();
        ValidateTitle(title);

        Title = title;
        Description = description;

        _tags.Clear();
        _tags.AddRange(TagList.Create(tags).Values);

        Touch();
    }

    public void Publish()
    {
        if (Status != QuizStatus.Draft)
        {
            throw new InvalidQuizException("只有草稿狀態的題庫可以發布。");
        }

        if (QuestionCount == 0)
        {
            throw new InvalidQuizException("題庫至少需要一題才能發布。");
        }

        Status = QuizStatus.Published;
        Touch();
    }

    public void Unpublish()
    {
        if (Status != QuizStatus.Published)
        {
            throw new InvalidQuizException("只有已發布狀態的題庫可以取消發布。");
        }

        Status = QuizStatus.Draft;
        Touch();
    }

    public void Archive()
    {
        if (Status == QuizStatus.Archived)
        {
            throw new InvalidQuizException("題庫已經是封存狀態。");
        }

        Status = QuizStatus.Archived;
        Touch();
    }

    public void RecordChallengeOutcome(bool passed)
    {
        Stats = Stats.RecordOutcome(passed);
    }

    private void EnsureEditable()
    {
        if (Status == QuizStatus.Archived)
        {
            throw new InvalidQuizException("封存狀態的題庫無法修改。");
        }
    }

    private void EnsureQuestionsEditable()
    {
        EnsureEditable();

        if (Status == QuizStatus.Published)
        {
            throw new InvalidQuizException("已發布的題庫無法修改題目，請先取消發布。");
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidQuizException("題庫標題不能為空。");
        }
    }
}