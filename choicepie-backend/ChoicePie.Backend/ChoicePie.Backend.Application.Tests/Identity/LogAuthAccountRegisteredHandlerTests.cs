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
}
