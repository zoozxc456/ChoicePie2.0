using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Shared.Application.Events;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class InvalidateQuizCacheHandlersTests
{
    private IQuizCacheInvalidator _invalidator = null!;
    private readonly Guid _quizId = Guid.NewGuid();

    [SetUp]
    public void SetUp() => _invalidator = Substitute.For<IQuizCacheInvalidator>();

    [Test]
    public async Task Handle_GivenDetailsUpdatedWithoutTagChange_WhenHandled_ThenInvalidatesOnlyThatQuiz()
    {
        var sut = new InvalidateQuizCacheOnDetailsUpdatedHandler(_invalidator);

        await sut.Handle(new DomainEventNotification<QuizDetailsUpdatedDomainEvent>(
            new QuizDetailsUpdatedDomainEvent(_quizId, TagsChanged: false)), CancellationToken.None);

        await _invalidator.Received(1).InvalidateQuizAsync(_quizId, Arg.Any<CancellationToken>());
        await _invalidator.DidNotReceive().InvalidateCatalogAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenDetailsUpdatedWithTagChange_WhenHandled_ThenInvalidatesQuizAndCatalog()
    {
        var sut = new InvalidateQuizCacheOnDetailsUpdatedHandler(_invalidator);

        await sut.Handle(new DomainEventNotification<QuizDetailsUpdatedDomainEvent>(
            new QuizDetailsUpdatedDomainEvent(_quizId, TagsChanged: true)), CancellationToken.None);

        await _invalidator.Received(1).InvalidateQuizAsync(_quizId, Arg.Any<CancellationToken>());
        await _invalidator.Received(1).InvalidateCatalogAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenCoverUpdated_WhenHandled_ThenInvalidatesOnlyThatQuiz()
    {
        var sut = new InvalidateQuizCacheOnCoverUpdatedHandler(_invalidator);

        await sut.Handle(new DomainEventNotification<QuizCoverUpdatedDomainEvent>(
            new QuizCoverUpdatedDomainEvent(_quizId)), CancellationToken.None);

        await _invalidator.Received(1).InvalidateQuizAsync(_quizId, Arg.Any<CancellationToken>());
        await _invalidator.DidNotReceive().InvalidateCatalogAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenQuestionsChanged_WhenHandled_ThenInvalidatesOnlyThatQuiz()
    {
        var sut = new InvalidateQuizCacheOnQuestionsChangedHandler(_invalidator);

        await sut.Handle(new DomainEventNotification<QuizQuestionsChangedDomainEvent>(
            new QuizQuestionsChangedDomainEvent(_quizId)), CancellationToken.None);

        await _invalidator.Received(1).InvalidateQuizAsync(_quizId, Arg.Any<CancellationToken>());
        await _invalidator.DidNotReceive().InvalidateCatalogAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenStatusChanged_WhenHandled_ThenInvalidatesQuizAndCatalog()
    {
        var sut = new InvalidateQuizCacheOnStatusChangedHandler(_invalidator);

        await sut.Handle(new DomainEventNotification<QuizStatusChangedDomainEvent>(
                new QuizStatusChangedDomainEvent(_quizId, QuizStatus.Draft, QuizStatus.Published)),
            CancellationToken.None);

        await _invalidator.Received(1).InvalidateQuizAsync(_quizId, Arg.Any<CancellationToken>());
        await _invalidator.Received(1).InvalidateCatalogAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenMemberProfileChanged_WhenHandled_ThenInvalidatesQuizzesOfThatCreator()
    {
        var memberId = Guid.NewGuid();
        var sut = new InvalidateQuizCacheOnCreatorProfileChangedHandler(_invalidator);

        await sut.Handle(new DomainEventNotification<MemberProfileChangedDomainEvent>(
            new MemberProfileChangedDomainEvent(memberId)), CancellationToken.None);

        await _invalidator.Received(1).InvalidateCreatorAsync(memberId, Arg.Any<CancellationToken>());
    }
}
