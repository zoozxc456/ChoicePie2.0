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

    [Test]
    public async Task Handle_GivenMemberCreatedEvent_WhenCalled_ThenDoesNotLogName()
    {
        var logger = Substitute.For<ILogger<LogMemberCreatedHandler>>();
        var sut = new LogMemberCreatedHandler(logger);
        var notification = new DomainEventNotification<MemberCreatedDomainEvent>(
            new MemberCreatedDomainEvent(Guid.NewGuid(), "Host Name"));

        await sut.Handle(notification, CancellationToken.None);

        // Log<TState> 的第三個引數是格式化後的 state，ToString() 即為輸出的訊息。
        var loggedMessages = logger.ReceivedCalls().Select(call => call.GetArguments()[2]?.ToString()).ToList();
        Assert.That(loggedMessages, Is.Not.Empty);
        Assert.That(loggedMessages, Has.None.Contains("Host Name"));
    }
}
