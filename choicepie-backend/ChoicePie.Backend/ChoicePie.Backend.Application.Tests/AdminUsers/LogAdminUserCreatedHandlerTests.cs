using ChoicePie.Backend.Application.AdminUsers.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Events;
using ChoicePie.Backend.Shared.Application.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class LogAdminUserCreatedHandlerTests
{
    [Test]
    public void Handle_GivenAdminUserCreatedEvent_WhenCalled_ThenCompletesWithoutThrowing()
    {
        var logger = Substitute.For<ILogger<LogAdminUserCreatedHandler>>();
        var sut = new LogAdminUserCreatedHandler(logger);
        var notification = new DomainEventNotification<AdminUserCreatedDomainEvent>(
            new AdminUserCreatedDomainEvent(Guid.NewGuid(), "Ops Name", "staff"));

        Assert.DoesNotThrowAsync(() => sut.Handle(notification, CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenAdminUserCreatedEvent_WhenCalled_ThenDoesNotLogName()
    {
        var logger = Substitute.For<ILogger<LogAdminUserCreatedHandler>>();
        var sut = new LogAdminUserCreatedHandler(logger);
        var notification = new DomainEventNotification<AdminUserCreatedDomainEvent>(
            new AdminUserCreatedDomainEvent(Guid.NewGuid(), "Ops Name", "staff"));

        await sut.Handle(notification, CancellationToken.None);

        // Log<TState> 的第三個引數是格式化後的 state，ToString() 即為輸出的訊息。
        var loggedMessages = logger.ReceivedCalls().Select(call => call.GetArguments()[2]?.ToString()).ToList();
        Assert.That(loggedMessages, Is.Not.Empty);
        Assert.That(loggedMessages, Has.None.Contains("Ops Name"));
    }
}
