using ChoicePie.Backend.Application.AdminUsers.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Events;
using ChoicePie.Backend.Shared.Application.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class LogAdminAuthAccountCreatedHandlerTests
{
    [Test]
    public void Handle_GivenAdminAuthAccountCreatedEvent_WhenCalled_ThenCompletesWithoutThrowing()
    {
        var logger = Substitute.For<ILogger<LogAdminAuthAccountCreatedHandler>>();
        var sut = new LogAdminAuthAccountCreatedHandler(logger);
        var notification = new DomainEventNotification<AdminAuthAccountCreatedDomainEvent>(
            new AdminAuthAccountCreatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "admin@example.com"));

        Assert.DoesNotThrowAsync(() => sut.Handle(notification, CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenAdminAuthAccountCreatedEvent_WhenCalled_ThenDoesNotLogEmail()
    {
        var logger = Substitute.For<ILogger<LogAdminAuthAccountCreatedHandler>>();
        var sut = new LogAdminAuthAccountCreatedHandler(logger);
        var notification = new DomainEventNotification<AdminAuthAccountCreatedDomainEvent>(
            new AdminAuthAccountCreatedDomainEvent(Guid.NewGuid(), Guid.NewGuid(), "admin@example.com"));

        await sut.Handle(notification, CancellationToken.None);

        // Log<TState> 的第三個引數是格式化後的 state，ToString() 即為輸出的訊息。
        var loggedMessages = logger.ReceivedCalls().Select(call => call.GetArguments()[2]?.ToString()).ToList();
        Assert.That(loggedMessages, Is.Not.Empty);
        Assert.That(loggedMessages, Has.None.Contains("admin@example.com"));
    }
}
