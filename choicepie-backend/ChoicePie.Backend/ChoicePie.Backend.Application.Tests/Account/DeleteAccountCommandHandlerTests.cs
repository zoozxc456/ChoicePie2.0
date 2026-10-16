using ChoicePie.Backend.Application.Account.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.Account;

[TestFixture]
public class DeleteAccountCommandHandlerTests
{
    private static readonly HashedPassword Hash = HashedPassword.Create("hash", "salt");

    private IMemberRepository _memberRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private DeleteAccountCommandHandler _sut = null!;
    private Member _member = null!;
    private RefreshTokenAggregate _token = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new DeleteAccountCommandHandler(_memberRepository, _authAccountRepository, _refreshTokenRepository,
            _passwordHasher, _currentUserService, _unitOfWork, timeProvider);

        _member = Member.Create("Host Name");
        _currentUserService.UserId.Returns(_member.Id);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
        _token = RefreshTokenAggregate.Issue(_member.Id, RefreshTokenOwnerType.Member, "hash", DateTime.UtcNow);
        _refreshTokenRepository
            .ListAsync(Arg.Any<ActiveRefreshTokensByOwnerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<RefreshTokenAggregate> { _token });
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private AuthAccount GivenAccount(bool withPassword)
    {
        var email = Email.Create("host@example.com");
        var account = withPassword
            ? AuthAccount.Register(email, Hash, _member.Id)
            : AuthAccount.RegisterWithExternalLogin(email, LoginProvider.Google, "sub", _member.Id);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        return account;
    }

    [Test]
    public async Task Handle_GivenCorrectPassword_WhenCalled_ThenSoftDeletesBothAggregatesAndRevokesTokens()
    {
        var account = GivenAccount(withPassword: true);
        _passwordHasher.Verify("pw", Hash).Returns(true);

        await _sut.Handle(new DeleteAccountCommand { Password = "pw" }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_member.IsDeleted, Is.True);
            Assert.That(account.IsDeleted, Is.True);
            Assert.That(_token.RevokedAt, Is.Not.Null);
            Assert.That(_member.DomainEvents.OfType<MemberAccountClosedDomainEvent>(), Is.Not.Empty);
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenWrongPassword_WhenCalled_ThenThrowsAndDeletesNothing()
    {
        var account = GivenAccount(withPassword: true);
        _passwordHasher.Verify("bad", Hash).Returns(false);

        Assert.ThrowsAsync<InvalidCurrentPasswordException>(() =>
            _sut.Handle(new DeleteAccountCommand { Password = "bad" }, CancellationToken.None));
        Assert.Multiple(() =>
        {
            Assert.That(_member.IsDeleted, Is.False);
            Assert.That(account.IsDeleted, Is.False);
            Assert.That(_token.RevokedAt, Is.Null);
        });
    }

    [Test]
    public void Handle_GivenAccountWithPasswordAndOnlyEmail_WhenCalled_ThenThrowsInvalidCurrentPasswordException()
    {
        GivenAccount(withPassword: true);

        Assert.ThrowsAsync<InvalidCurrentPasswordException>(() =>
            _sut.Handle(new DeleteAccountCommand { ConfirmEmail = "host@example.com" }, CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenGoogleOnlyAccountAndMatchingEmail_WhenCalled_ThenDeletesAccount()
    {
        var account = GivenAccount(withPassword: false);

        await _sut.Handle(new DeleteAccountCommand { ConfirmEmail = " Host@Example.com " }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_member.IsDeleted, Is.True);
            Assert.That(account.IsDeleted, Is.True);
        });
    }

    [TestCase("other@example.com")]
    [TestCase("")]
    [TestCase(null)]
    public void Handle_GivenGoogleOnlyAccountAndWrongEmail_WhenCalled_ThenThrowsInvalidAccountDeletionConfirmationException(
        string? email)
    {
        GivenAccount(withPassword: false);

        Assert.ThrowsAsync<InvalidAccountDeletionConfirmationException>(() =>
            _sut.Handle(new DeleteAccountCommand { ConfirmEmail = email }, CancellationToken.None));
        Assert.That(_member.IsDeleted, Is.False);
    }
}
