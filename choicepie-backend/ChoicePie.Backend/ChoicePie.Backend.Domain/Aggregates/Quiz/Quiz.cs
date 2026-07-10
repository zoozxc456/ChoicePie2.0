using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz;

public sealed class Quiz : AggregateRoot<Guid>
{
    private readonly List<Question> _questions = [];
    private readonly List<string> _tags = [];

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string CoverEmoji { get; private set; } = null!;
    public string CoverGradient { get; private set; } = null!;
    public Difficulty Difficulty { get; private set; } = null!;
    public bool IsPublic { get; private set; }

    // Reuses the inherited audit CreatorId (set by SetCreated below) rather than storing the
    // owner twice - the creator IS the owner for authorization purposes in this phase.
    [NotMapped]
    public Guid OwnerId => CreatorId!.Value;

    // Derived from a future History bounded context this aggregate can't see yet. Updated by a
    // future Quiz.RecordChallengeOutcome(...) called from an INotificationHandler in a later
    // phase. EfUnitOfWork dispatches domain events after commit, outside any transaction, so that
    // future handler must be idempotent (at-least-once delivery) - don't assume exactly-once.
    public int ChallengeCount { get; private set; }
    public decimal PassRate { get; private set; }

    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    // Fully derivable from data this aggregate already owns - must stay computed, not persisted,
    // or it becomes a second source of truth alongside Questions.
    [NotMapped]
    public int QuestionCount => _questions.Count;

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
        bool isPublic,
        IEnumerable<string> tags)
    {
        ValidateTitle(title);

        var quiz = new Quiz
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            CoverEmoji = coverEmoji,
            CoverGradient = coverGradient,
            Difficulty = difficulty,
            IsPublic = isPublic,
            ChallengeCount = 0,
            PassRate = 0
        };
        quiz._tags.AddRange(tags.Distinct(StringComparer.OrdinalIgnoreCase));

        quiz.SetCreated(creatorId);
        quiz.AddDomainEvent(new QuizCreatedDomainEvent(quiz.Id, creatorId, quiz.Title));

        return quiz;
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
        _questions.Add(question);
        Touch();
    }

    public void RemoveQuestion(Guid questionId)
    {
        _questions.RemoveAll(q => q.Id == questionId);
        Touch();
    }

    public void UpdateDetails(string title, string? description, bool isPublic, IEnumerable<string> tags)
    {
        ValidateTitle(title);

        Title = title;
        Description = description;
        IsPublic = isPublic;

        _tags.Clear();
        _tags.AddRange(tags.Distinct(StringComparer.OrdinalIgnoreCase));

        Touch();
    }

    public void Publish()
    {
        IsPublic = true;
        Touch();
    }

    public void Unpublish()
    {
        IsPublic = false;
        Touch();
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new InvalidQuizException("題庫標題不能為空。");
        }
    }
}
