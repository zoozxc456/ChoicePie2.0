using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class LoginCommandHandlerTests
{
    private IAuthAccountRepository _authAccountRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private ITokenService _tokenService = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private LoginCommandHandler _sut = null!;
    private Member _registeredMember = null!;
    private AuthAccount _registeredAuthAccount = null!;

    [SetUp]
    public void SetUp()
    {
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<ITokenService>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new LoginCommandHandler(_authAccountRepository, _memberRepository, _refreshTokenRepository,
            _passwordHasher, _tokenService, _refreshTokenGenerator, _unitOfWork);

        _registeredMember = Member.Create("Host Name");
        _registeredAuthAccount = AuthAccount.Register(
            Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), _registeredMember.Id);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_registeredAuthAccount);
        _memberRepository.GetByIdAsync(_registeredMember.Id, Arg.Any<CancellationToken>()).Returns(_registeredMember);
        _refreshTokenGenerator.Generate().Returns(("raw-refresh-token", "refresh-token-hash"));
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static LoginCommand ValidCommand() => new() { Email = "host@example.com", Password = "correct-password" };

    [Test]
    public async Task Handle_GivenValidCredentials_WhenCalled_ThenReturnsMemberAndTokens()
    {
        _passwordHasher.Verify("correct-password", HashedPassword.Create("hashed-password", "salt")).Returns(true);
        _tokenService.GenerateAccessToken(_registeredMember).Returns("jwt-access-token");

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.EqualTo("jwt-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("raw-refresh-token"));
            Assert.That(result.Member.Email, Is.EqualTo("host@example.com"));
        });
    }

    [Test]
    public async Task Handle_GivenValidCredentials_WhenCalled_ThenPersistsRefreshTokenForMember()
    {
        _passwordHasher.Verify("correct-password", HashedPassword.Create("hashed-password", "salt")).Returns(true);
        _tokenService.GenerateAccessToken(_registeredMember).Returns("jwt-access-token");

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshTokenAggregate>(t => t.OwnerId == _registeredMember.Id && t.TokenHash == "refresh-token-hash"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownEmail_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AuthAccount?)null);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenWrongPassword_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>()).Returns(false);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
