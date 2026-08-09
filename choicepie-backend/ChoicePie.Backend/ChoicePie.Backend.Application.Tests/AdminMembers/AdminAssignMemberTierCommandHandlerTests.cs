using ChoicePie.Backend.Application.AdminMembers.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;
using MembershipTierAggregate = ChoicePie.Backend.Domain.Aggregates.MembershipTier.MembershipTier;

namespace ChoicePie.Backend.Application.Tests.AdminMembers;

[TestFixture]
public class AdminAssignMemberTierCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IMembershipTierRepository _membershipTierRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminAssignMemberTierCommandHandler _sut = null!;
    private MemberAggregate _member = null!;
    private MembershipTierAggregate _tier = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _membershipTierRepository = Substitute.For<IMembershipTierRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminAssignMemberTierCommandHandler(_memberRepository, _membershipTierRepository, _unitOfWork);

        _member = MemberAggregate.Create("Host Name");
        _tier = MembershipTierAggregate.Create("Pro", 10, 50_000);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
        _membershipTierRepository.GetByIdAsync(_tier.Id, Arg.Any<CancellationToken>()).Returns(_tier);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidTier_WhenCalled_ThenAssignsAndPersists()
    {
        await _sut.Handle(new AdminAssignMemberTierCommand(_member.Id, _tier.Id), CancellationToken.None);

        Assert.That(_member.TierId, Is.EqualTo(_tier.Id));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenMemberNotFound_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _memberRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((MemberAggregate?)null);

        Assert.ThrowsAsync<MemberNotFoundException>(() =>
            _sut.Handle(new AdminAssignMemberTierCommand(missingId, _tier.Id), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenTierNotFound_WhenCalled_ThenThrowsMembershipTierNotFoundException()
    {
        var missingTierId = Guid.NewGuid();
        _membershipTierRepository.GetByIdAsync(missingTierId, Arg.Any<CancellationToken>()).Returns((MembershipTierAggregate?)null);

        Assert.ThrowsAsync<MembershipTierNotFoundException>(() =>
            _sut.Handle(new AdminAssignMemberTierCommand(_member.Id, missingTierId), CancellationToken.None));
    }
}
