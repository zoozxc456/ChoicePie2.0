using ChoicePie.Backend.Application.Identity.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Events;
using ChoicePie.Backend.Shared.Application.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class LogAuthAccountRegisteredHandlerTests
{
    [Test]
    public void Handle_GivenAuthAccountRegisteredEvent_WhenCalled_ThenCompletesWithoutThrowing()
    {
        var logger = Substitute.For<ILogger<LogAuthAccountRegisteredHandler>>();
        var sut = new LogAuthAccountRegisteredHandler(logger);
        var notification = new DomainEventNotification<AuthAccountRegisteredDomainEvent>(
            new AuthAccountRegisteredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "host@example.com"));

        Assert.DoesNotThrowAsync(() => sut.Handle(notification, CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenAuthAccountRegisteredEvent_WhenCalled_ThenDoesNotLogEmail()
    {
        var logger = Substitute.For<ILogger<LogAuthAccountRegisteredHandler>>();
        var sut = new LogAuthAccountRegisteredHandler(logger);
        var notification = new DomainEventNotification<AuthAccountRegisteredDomainEvent>(
            new AuthAccountRegisteredDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "host@example.com"));

        await sut.Handle(notification, CancellationToken.None);

        // Log<TState> 的第三個引數是格式化後的 state，ToString() 即為輸出的訊息。
        var loggedMessages = logger.ReceivedCalls().Select(call => call.GetArguments()[2]?.ToString()).ToList();
        Assert.That(loggedMessages, Is.Not.Empty);
        Assert.That(loggedMessages, Has.None.Contains("host@example.com"));
    }
}
