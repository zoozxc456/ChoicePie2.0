using ChoicePie.Backend.Application.Quizzes.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using QuizAggregate = ChoicePie.Backend.Domain.Aggregates.Quiz.Quiz;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class ArchivePublishedQuizzesOnAccountClosedHandlerTests
{
    [Test]
    public async Task Handle_GivenMemberWithPublishedQuizzes_WhenAccountClosed_ThenArchivesThem()
    {
        var memberId = Guid.NewGuid();
        var quiz = QuizAggregate.Create(memberId, "Title", null, null, "x", "primary", Difficulty.Beginner, ["tag"]);
        quiz.AddQuestion(Question.Create("2+2=?", ["1", "2", "3", "4"], 3, "basic math"));
        quiz.Publish();
        var quizRepository = Substitute.For<IQuizRepository>();
        quizRepository.ListAsync(Arg.Any<PublishedQuizzesByCreatorSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<QuizAggregate> { quiz });
        using var unitOfWork = Substitute.For<IUnitOfWork>();
        var sut = new ArchivePublishedQuizzesOnAccountClosedHandler(quizRepository, unitOfWork);

        await sut.Handle(new ReliableDomainEventNotification<MemberAccountClosedDomainEvent>(
            new MemberAccountClosedDomainEvent(memberId)), CancellationToken.None);

        Assert.That(quiz.Status, Is.EqualTo(QuizStatus.Archived));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenMemberWithoutPublishedQuizzes_WhenAccountClosed_ThenDoesNotSave()
    {
        var quizRepository = Substitute.For<IQuizRepository>();
        quizRepository.ListAsync(Arg.Any<PublishedQuizzesByCreatorSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<QuizAggregate>());
        using var unitOfWork = Substitute.For<IUnitOfWork>();
        var sut = new ArchivePublishedQuizzesOnAccountClosedHandler(quizRepository, unitOfWork);

        await sut.Handle(new ReliableDomainEventNotification<MemberAccountClosedDomainEvent>(
            new MemberAccountClosedDomainEvent(Guid.NewGuid())), CancellationToken.None);

        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
