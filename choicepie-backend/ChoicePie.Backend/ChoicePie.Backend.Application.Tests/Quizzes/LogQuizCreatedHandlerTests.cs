using ChoicePie.Backend.Application.Quizzes.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Shared.Application.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class LogQuizCreatedHandlerTests
{
    [Test]
    public void Handle_GivenQuizCreatedEvent_WhenCalled_ThenCompletesWithoutThrowing()
    {
        var logger = Substitute.For<ILogger<LogQuizCreatedHandler>>();
        var sut = new LogQuizCreatedHandler(logger);
        var notification = new DomainEventNotification<QuizCreatedDomainEvent>(
            new QuizCreatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "Kubernetes 101"));

        Assert.DoesNotThrowAsync(() => sut.Handle(notification, CancellationToken.None));
    }
}
