using ChoicePie.Backend.Application.Identity.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Shared.Application.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class LogMemberRegisteredHandlerTests
{
    [Test]
    public void Handle_GivenMemberRegisteredEvent_WhenCalled_ThenCompletesWithoutThrowing()
    {
        var logger = Substitute.For<ILogger<LogMemberRegisteredHandler>>();
        var sut = new LogMemberRegisteredHandler(logger);
        var notification = new DomainEventNotification<MemberRegisteredDomainEvent>(
            new MemberRegisteredDomainEvent(Guid.NewGuid(), "host@example.com", "Host Name"));

        Assert.DoesNotThrowAsync(() => sut.Handle(notification, CancellationToken.None));
    }
}
