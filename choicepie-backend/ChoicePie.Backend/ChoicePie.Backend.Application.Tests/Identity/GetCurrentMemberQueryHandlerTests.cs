using ChoicePie.Backend.Application.Identity.Queries;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class GetCurrentMemberQueryHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetCurrentMemberQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetCurrentMemberQueryHandler(_memberRepository, _authAccountRepository, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedMemberWithMatchingRecord_WhenCalled_ThenReturnsMemberDto()
    {
        var member = Member.Create("Host Name");
        var authAccount = AuthAccount.Register(Email.Create("host@example.com"), "hashed-password", member.Id);
        _currentUserService.UserId.Returns(member.Id);
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _authAccountRepository.FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(authAccount);

        var result = await _sut.Handle(new GetCurrentMemberQuery(), CancellationToken.None);

        Assert.That(result.Email, Is.EqualTo("host@example.com"));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new GetCurrentMemberQuery(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCurrentUserIdHasNoMatchingRecord_WhenCalled_ThenThrowsMemberNotFoundException()
    {
        var memberId = Guid.NewGuid();
        _currentUserService.UserId.Returns(memberId);
        _memberRepository.GetByIdAsync(memberId, Arg.Any<CancellationToken>()).Returns((Member?)null);

        Assert.ThrowsAsync<MemberNotFoundException>(() => _sut.Handle(new GetCurrentMemberQuery(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenMemberWithNoMatchingAuthAccount_WhenCalled_ThenThrowsAuthAccountNotFoundException()
    {
        var member = Member.Create("Host Name");
        _currentUserService.UserId.Returns(member.Id);
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);
        _authAccountRepository.FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AuthAccount?)null);

        Assert.ThrowsAsync<AuthAccountNotFoundException>(() => _sut.Handle(new GetCurrentMemberQuery(), CancellationToken.None));
    }
}
