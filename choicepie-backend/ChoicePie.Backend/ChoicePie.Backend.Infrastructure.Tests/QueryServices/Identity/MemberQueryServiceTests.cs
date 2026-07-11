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
}