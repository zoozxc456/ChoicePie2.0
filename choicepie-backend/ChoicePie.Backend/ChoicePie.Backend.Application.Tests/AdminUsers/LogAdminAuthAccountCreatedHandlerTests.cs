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
}
