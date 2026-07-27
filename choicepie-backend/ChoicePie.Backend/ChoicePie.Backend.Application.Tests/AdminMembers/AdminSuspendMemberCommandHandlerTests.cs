using ChoicePie.Backend.Application.AdminMembers.Commands;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using NSubstitute;
using MemberAggregate = ChoicePie.Backend.Domain.Aggregates.Member.Member;

namespace ChoicePie.Backend.Application.Tests.AdminMembers;

[TestFixture]
public class AdminSuspendMemberCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminSuspendMemberCommandHandler _sut = null!;
    private MemberAggregate _member = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminSuspendMemberCommandHandler(_memberRepository, _unitOfWork);

        _member = MemberAggregate.Create("Host Name");
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    [Test]
    public async Task Handle_GivenValidReason_WhenCalled_ThenSuspendsAndPersists()
    {
        var until = DateTime.UtcNow.AddDays(7);

        await _sut.Handle(new AdminSuspendMemberCommand(_member.Id, "spamming", until), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_member.IsSuspended, Is.True);
            Assert.That(_member.SuspendedReason, Is.EqualTo("spamming"));
            Assert.That(_member.SuspendedUntil, Is.EqualTo(until));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenMemberNotFound_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        var missingId = Guid.NewGuid();
        _memberRepository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((MemberAggregate?)null);

        Assert.ThrowsAsync<MemberNotFoundException>(() =>
            _sut.Handle(new AdminSuspendMemberCommand(missingId, "spamming", null), CancellationToken.None));
    }
}
