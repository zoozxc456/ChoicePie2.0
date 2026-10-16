using ChoicePie.Backend.Application.Account.Commands;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
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
public class ChangePasswordCommandHandlerTests
{
    private static readonly Guid MemberId = Guid.NewGuid();
    private static readonly HashedPassword OldHash = HashedPassword.Create("old-hash", "old-salt");
    private static readonly HashedPassword NewHash = HashedPassword.Create("new-hash", "new-salt");

    private IAuthAccountRepository _authAccountRepository = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private ChangePasswordCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new ChangePasswordCommandHandler(_authAccountRepository, _refreshTokenRepository, _passwordHasher,
            _refreshTokenGenerator, _currentUserService, _unitOfWork, timeProvider);

        _currentUserService.UserId.Returns(MemberId);
        _passwordHasher.Hash("new-password-1").Returns(NewHash);
        _refreshTokenRepository
            .ListAsync(Arg.Any<ActiveRefreshTokensByOwnerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<RefreshTokenAggregate>());
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private AuthAccount GivenPasswordAccount()
    {
        var account = AuthAccount.Register(Email.Create("a@example.com"), OldHash, MemberId);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        return account;
    }

    private AuthAccount GivenGoogleOnlyAccount()
    {
        var account = AuthAccount.RegisterWithExternalLogin(
            Email.Create("a@example.com"), LoginProvider.Google, "sub", MemberId);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(account);
        return account;
    }

    [Test]
    public async Task Handle_GivenCorrectCurrentPassword_WhenCalled_ThenChangesPassword()
    {
        var account = GivenPasswordAccount();
        _passwordHasher.Verify("old-password", OldHash).Returns(true);

        await _sut.Handle(new ChangePasswordCommand { CurrentPassword = "old-password", NewPassword = "new-password-1" },
            CancellationToken.None);

        Assert.That(account.OriginalPassword, Is.EqualTo(NewHash));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenWrongCurrentPassword_WhenCalled_ThenThrowsInvalidCurrentPasswordException()
    {
        var account = GivenPasswordAccount();
        _passwordHasher.Verify("wrong", OldHash).Returns(false);

        Assert.ThrowsAsync<InvalidCurrentPasswordException>(() => _sut.Handle(
            new ChangePasswordCommand { CurrentPassword = "wrong", NewPassword = "new-password-1" },
            CancellationToken.None));
        Assert.That(account.OriginalPassword, Is.EqualTo(OldHash));
    }

    [Test]
    public void Handle_GivenAccountWithPasswordAndNoCurrentPassword_WhenCalled_ThenThrowsInvalidCurrentPasswordException()
    {
        GivenPasswordAccount();

        Assert.ThrowsAsync<InvalidCurrentPasswordException>(() => _sut.Handle(
            new ChangePasswordCommand { NewPassword = "new-password-1" }, CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenGoogleOnlyAccount_WhenCalled_ThenSetsPasswordWithoutCurrentPassword()
    {
        var account = GivenGoogleOnlyAccount();

        await _sut.Handle(new ChangePasswordCommand { NewPassword = "new-password-1" }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(account.HasPassword, Is.True);
            Assert.That(account.OriginalPassword, Is.EqualTo(NewHash));
        });
    }

    [Test]
    public async Task Handle_GivenOtherActiveSessions_WhenCalled_ThenRevokesOthersAndKeepsCurrentSession()
    {
        GivenGoogleOnlyAccount();
        var current = RefreshTokenAggregate.Issue(MemberId, RefreshTokenOwnerType.Member, "current-hash", DateTime.UtcNow);
        var other = RefreshTokenAggregate.Issue(MemberId, RefreshTokenOwnerType.Member, "other-hash", DateTime.UtcNow);
        _refreshTokenGenerator.Hash("current-raw").Returns("current-hash");
        _refreshTokenRepository
            .ListAsync(Arg.Any<ActiveRefreshTokensByOwnerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<RefreshTokenAggregate> { current, other });

        await _sut.Handle(
            new ChangePasswordCommand { NewPassword = "new-password-1", CurrentRefreshToken = "current-raw" },
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(current.RevokedAt, Is.Null);
            Assert.That(other.RevokedAt, Is.Not.Null);
        });
    }

    [Test]
    public async Task Handle_GivenNoCurrentRefreshToken_WhenCalled_ThenRevokesAllSessions()
    {
        GivenGoogleOnlyAccount();
        var token = RefreshTokenAggregate.Issue(MemberId, RefreshTokenOwnerType.Member, "any-hash", DateTime.UtcNow);
        _refreshTokenRepository
            .ListAsync(Arg.Any<ActiveRefreshTokensByOwnerSpecification>(), Arg.Any<CancellationToken>())
            .Returns(new List<RefreshTokenAggregate> { token });

        await _sut.Handle(new ChangePasswordCommand { NewPassword = "new-password-1" }, CancellationToken.None);

        Assert.That(token.RevokedAt, Is.Not.Null);
    }

    [Test]
    public void Handle_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(
            new ChangePasswordCommand { NewPassword = "new-password-1" }, CancellationToken.None));
    }
}
