using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Application.Identity.EventHandlers;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Shared.Application.Events;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class InvalidateMemberActivityCacheHandlersTests
{
    private IMemberActivityCache _cache = null!;
    private readonly Guid _memberId = Guid.NewGuid();

    [SetUp]
    public void SetUp() => _cache = Substitute.For<IMemberActivityCache>();

    [Test]
    public async Task Handle_GivenAccountClosed_WhenHandled_ThenInvalidatesCacheOfThatMember()
    {
        var sut = new InvalidateMemberActivityCacheOnAccountClosedHandler(_cache);

        await sut.Handle(new DomainEventNotification<MemberAccountClosedDomainEvent>(
            new MemberAccountClosedDomainEvent(_memberId)), CancellationToken.None);

        await _cache.Received(1).InvalidateAsync(_memberId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenMemberSuspended_WhenHandled_ThenInvalidatesCacheOfThatMember()
    {
        var sut = new InvalidateMemberActivityCacheOnSuspendedHandler(_cache);

        await sut.Handle(new DomainEventNotification<MemberSuspendedDomainEvent>(
            new MemberSuspendedDomainEvent(_memberId)), CancellationToken.None);

        await _cache.Received(1).InvalidateAsync(_memberId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenMemberUnsuspended_WhenHandled_ThenInvalidatesCacheOfThatMember()
    {
        var sut = new InvalidateMemberActivityCacheOnUnsuspendedHandler(_cache);

        await sut.Handle(new DomainEventNotification<MemberUnsuspendedDomainEvent>(
            new MemberUnsuspendedDomainEvent(_memberId)), CancellationToken.None);

        await _cache.Received(1).InvalidateAsync(_memberId, Arg.Any<CancellationToken>());
    }
}
