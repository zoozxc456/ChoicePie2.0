using ChoicePie.Backend.Application.AdminMembers.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;

namespace ChoicePie.Backend.Application.Tests.AdminMembers;

[TestFixture]
public class AdminUnsuspendMemberCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminUnsuspendMemberCommandHandler _sut = null!;
    private MemberAggregate _member = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminUnsuspendMemberCommandHandler(_memberRepository, _unitOfWork);

        _member = MemberAggregate.Create("Host Name");
        _member.Suspend("spamming", null);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenSuspendedMember_WhenCalled_ThenUnsuspendsAndPersists()
    {
        await _sut.Handle(new AdminUnsuspendMemberCommand(_member.Id), CancellationToken.None);

        Assert.That(_member.IsSuspended, Is.False);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenMemberNotFound_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _memberRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((MemberAggregate?)null);

        Assert.ThrowsAsync<MemberNotFoundException>(() =>
            _sut.Handle(new AdminUnsuspendMemberCommand(missingId), CancellationToken.None));
    }
}
