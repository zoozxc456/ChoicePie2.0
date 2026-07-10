using ChoicePie.Backend.Application.Identity.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Shared.Application.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class LogMemberCreatedHandlerTests
{
    [Test]
    public void Handle_GivenMemberCreatedEvent_WhenCalled_ThenCompletesWithoutThrowing()
    {
        var logger = Substitute.For<ILogger<LogMemberCreatedHandler>>();
        var sut = new LogMemberCreatedHandler(logger);
        var notification = new DomainEventNotification<MemberCreatedDomainEvent>(
            new MemberCreatedDomainEvent(Guid.NewGuid(), "Host Name"));

        Assert.DoesNotThrowAsync(() => sut.Handle(notification, CancellationToken.None));
    }
}
