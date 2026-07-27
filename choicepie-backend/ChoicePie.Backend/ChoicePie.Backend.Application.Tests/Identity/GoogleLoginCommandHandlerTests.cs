using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class GoogleLoginCommandHandlerTests
{
    private IGoogleIdTokenVerifier _googleIdTokenVerifier = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private ITokenService _tokenService = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private GoogleLoginCommandHandler _sut = null!;

    private static readonly GoogleIdentity Identity =
        new("google-subject-id", "google-user@example.com", "Google User", "https://example.com/pic.jpg");

    [SetUp]
    public void SetUp()
    {
        _googleIdTokenVerifier = Substitute.For<IGoogleIdTokenVerifier>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new GoogleLoginCommandHandler(_googleIdTokenVerifier, _authAccountRepository, _memberRepository,
            _refreshTokenRepository, _tokenService, _refreshTokenGenerator, _unitOfWork, _timeProvider);

        _googleIdTokenVerifier.VerifyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Identity);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByExternalIdentitySpecification>(), Arg.Any<CancellationToken>())
            .Returns((AuthAccount?)null);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AuthAccount?)null);
        _refreshTokenGenerator.Generate().Returns(("raw-refresh-token", "refresh-token-hash"));
        _tokenService.GenerateAccessToken(Arg.Any<Member>()).Returns("jwt-access-token");
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static GoogleLoginCommand ValidCommand() => new() { IdToken = "raw-id-token" };

    [Test]
    public async Task Handle_GivenNewGoogleUser_WhenCalled_ThenCreatesVerifiedMemberAndAuthAccount()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.EqualTo("jwt-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("raw-refresh-token"));
            Assert.That(result.Member.Email, Is.EqualTo("google-user@example.com"));
            Assert.That(result.Member.IsVerified, Is.True);
        });
        await _memberRepository.Received(1).AddAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _authAccountRepository.Received(1).AddAsync(
            Arg.Is<AuthAccount>(a => a.LoginMethods.Single().Provider == LoginProvider.Google),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenExistingGoogleAccount_WhenCalled_ThenLogsInWithoutCreatingNewAccount()
    {
        var member = Member.Create("Existing Member");
        var authAccount = AuthAccount.RegisterWithExternalLogin(
            Email.Create("google-user@example.com"), LoginProvider.Google, "google-subject-id", member.Id);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByExternalIdentitySpecification>(), Arg.Any<CancellationToken>())
            .Returns(authAccount);
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.Member.Email, Is.EqualTo("google-user@example.com"));
        await _memberRepository.DidNotReceive().AddAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
        await _authAccountRepository.DidNotReceive().AddAsync(Arg.Any<AuthAccount>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenEmailAlreadyRegisteredWithoutGoogle_WhenCalled_ThenLinksGoogleToExistingAccount()
    {
        var member = Member.Create("Existing Member");
        var authAccount = AuthAccount.Register(
            Email.Create("google-user@example.com"), HashedPassword.Create("hash", "salt"), member.Id);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(authAccount);
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.Member.Email, Is.EqualTo("google-user@example.com"));
        Assert.That(authAccount.LoginMethods.Any(m => m.Provider == LoginProvider.Google), Is.True);
        await _authAccountRepository.Received(1).UpdateAsync(authAccount, Arg.Any<CancellationToken>());
        await _memberRepository.DidNotReceive().AddAsync(Arg.Any<Member>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenValidLogin_WhenCalled_ThenPersistsRefreshToken()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshTokenAggregate>(t => t.TokenHash == "refresh-token-hash"), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenSuspendedMember_WhenCalled_ThenThrowsMemberSuspendedException()
    {
        var member = Member.Create("Existing Member");
        member.Suspend("spamming", null);
        var authAccount = AuthAccount.RegisterWithExternalLogin(
            Email.Create("google-user@example.com"), LoginProvider.Google, "google-subject-id", member.Id);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByExternalIdentitySpecification>(), Arg.Any<CancellationToken>())
            .Returns(authAccount);
        _memberRepository.GetByIdAsync(member.Id, Arg.Any<CancellationToken>()).Returns(member);

        Assert.ThrowsAsync<MemberSuspendedException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
