using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Infrastructure.QueryServices.Identity;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.Identity;

[TestFixture]
public class MemberQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private MemberQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new MemberQueryService(_readRepository);
    }

    [Test]
    public async Task GetByIdAsync_GivenMemberWithMatchingAuthAccount_WhenCalled_ThenReturnsMemberDto()
    {
        var member = Member.Create("Host Name");
        var authAccount = AuthAccount.Register(
            Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), member.Id);
        _readRepository.Query<Member>().Returns(new List<Member> { member }.AsQueryable());
        _readRepository.Query<AuthAccount>().Returns(new List<AuthAccount> { authAccount }.AsQueryable());

        var result = await _sut.GetByIdAsync(member.Id, CancellationToken.None);

        Assert.That(result.Email, Is.EqualTo("host@example.com"));
    }

    [Test]
    public void GetByIdAsync_GivenUnknownMemberId_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        _readRepository.Query<Member>().Returns(new List<Member>().AsQueryable());

        Assert.ThrowsAsync<MemberNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetByIdAsync_GivenMemberWithNoMatchingAuthAccount_WhenCalled_ThenThrowsAuthAccountNotFoundException()
    {
        var member = Member.Create("Host Name");
        _readRepository.Query<Member>().Returns(new List<Member> { member }.AsQueryable());
        _readRepository.Query<AuthAccount>().Returns(new List<AuthAccount>().AsQueryable());

        Assert.ThrowsAsync<AuthAccountNotFoundException>(() => _sut.GetByIdAsync(member.Id, CancellationToken.None));
    }

    [Test]
    public async Task AdminListAsync_GivenSuspendedAndActiveMembers_WhenCalled_ThenReturnsBothWithSuspensionFields()
    {
        var active = Member.Create("Active Member");
        var suspended = Member.Create("Suspended Member");
        suspended.Suspend("spamming", null);
        var activeAuth = AuthAccount.Register(
            Email.Create("active@example.com"), HashedPassword.Create("hashed-password", "salt"), active.Id);
        var suspendedAuth = AuthAccount.Register(
            Email.Create("suspended@example.com"), HashedPassword.Create("hashed-password", "salt"), suspended.Id);
        _readRepository.Query<Member>().Returns(new List<Member> { active, suspended }.AsQueryable());
        _readRepository.Query<AuthAccount>().Returns(new List<AuthAccount> { activeAuth, suspendedAuth }.AsQueryable());

        var result = await _sut.AdminListAsync(null, 1, 20, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(2));
        var suspendedDto = result.Items.Single(m => m.Id == suspended.Id);
        Assert.Multiple(() =>
        {
            Assert.That(suspendedDto.IsSuspended, Is.True);
            Assert.That(suspendedDto.SuspendedReason, Is.EqualTo("spamming"));
            Assert.That(suspendedDto.Email, Is.EqualTo("suspended@example.com"));
        });
    }

    [Test]
    public async Task AdminListAsync_GivenSearchNotMatchingAnyName_WhenCalled_ThenReturnsEmptyResult()
    {
        var member = Member.Create("Host Name");
        _readRepository.Query<Member>().Returns(new List<Member> { member }.AsQueryable());
        _readRepository.Query<AuthAccount>().Returns(new List<AuthAccount>().AsQueryable());

        var result = await _sut.AdminListAsync("no-match", 1, 20, CancellationToken.None);

        Assert.That(result.TotalCount, Is.EqualTo(0));
    }
}