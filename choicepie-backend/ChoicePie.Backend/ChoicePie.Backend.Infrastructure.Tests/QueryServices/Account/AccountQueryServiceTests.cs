using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Infrastructure.QueryServices.Account;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.Account;

[TestFixture]
public class AccountQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private AccountQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new AccountQueryService(_readRepository);
    }

    private void Given(Member member, AuthAccount authAccount)
    {
        _readRepository.Query<Member>().Returns(new List<Member> { member }.AsQueryable());
        _readRepository.Query<AuthAccount>().Returns(new List<AuthAccount> { authAccount }.AsQueryable());
    }

    [Test]
    public async Task GetByMemberIdAsync_GivenPasswordAccount_WhenCalled_ThenReportsPasswordAndNoGoogle()
    {
        var member = Member.Create("Host Name");
        Given(member, AuthAccount.Register(Email.Create("a@example.com"), HashedPassword.Create("h", "s"), member.Id));

        var result = await _sut.GetByMemberIdAsync(member.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo("a@example.com"));
            Assert.That(result.Name, Is.EqualTo("Host Name"));
            Assert.That(result.HasPassword, Is.True);
            Assert.That(result.GoogleLinked, Is.False);
        });
    }

    [Test]
    public async Task GetByMemberIdAsync_GivenGoogleOnlyAccount_WhenCalled_ThenReportsGoogleAndNoPassword()
    {
        var member = Member.Create("Host Name");
        Given(member, AuthAccount.RegisterWithExternalLogin(
            Email.Create("a@example.com"), LoginProvider.Google, "sub", member.Id));

        var result = await _sut.GetByMemberIdAsync(member.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasPassword, Is.False);
            Assert.That(result.GoogleLinked, Is.True);
            Assert.That(result.IsVerified, Is.True);
        });
    }

    [Test]
    public void GetByMemberIdAsync_GivenUnknownMember_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        _readRepository.Query<Member>().Returns(new List<Member>().AsQueryable());

        Assert.ThrowsAsync<MemberNotFoundException>(() => _sut.GetByMemberIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetByMemberIdAsync_GivenMemberWithoutAuthAccount_WhenCalled_ThenThrowsAuthAccountNotFoundException()
    {
        var member = Member.Create("Host Name");
        _readRepository.Query<Member>().Returns(new List<Member> { member }.AsQueryable());
        _readRepository.Query<AuthAccount>().Returns(new List<AuthAccount>().AsQueryable());

        Assert.ThrowsAsync<AuthAccountNotFoundException>(() => _sut.GetByMemberIdAsync(member.Id, CancellationToken.None));
    }
}
