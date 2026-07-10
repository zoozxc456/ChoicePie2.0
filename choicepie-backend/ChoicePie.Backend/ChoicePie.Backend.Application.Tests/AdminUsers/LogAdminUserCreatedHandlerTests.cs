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
}
