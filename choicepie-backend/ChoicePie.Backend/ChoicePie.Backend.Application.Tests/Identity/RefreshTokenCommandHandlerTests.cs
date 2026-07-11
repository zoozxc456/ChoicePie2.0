using ChoicePie.Backend.Application.Identity.Commands;
using ChoicePie.Backend.Application.Identity.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.Identity;

[TestFixture]
public class RefreshTokenCommandHandlerTests
{
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IAuthAccountRepository _authAccountRepository = null!;
    private IMemberRepository _memberRepository = null!;
    private ITokenService _tokenService = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private RefreshTokenCommandHandler _sut = null!;
    private Member _member = null!;
    private AuthAccount _authAccount = null!;
    private RefreshTokenAggregate _existingRefreshToken = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _authAccountRepository = Substitute.For<IAuthAccountRepository>();
        _memberRepository = Substitute.For<IMemberRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new RefreshTokenCommandHandler(_refreshTokenRepository, _authAccountRepository, _memberRepository,
            _tokenService, _refreshTokenGenerator, _unitOfWork);

        _member = Member.Create("Host Name");
        _authAccount = AuthAccount.Register(
            Email.Create("host@example.com"), HashedPassword.Create("hashed-password", "salt"), _member.Id);
        _existingRefreshToken =
            RefreshTokenAggregate.Issue(_member.Id, RefreshTokenOwnerType.Member, "old-hash", DateTime.UtcNow);

        _refreshTokenGenerator.Hash("valid-raw-token").Returns("old-hash");
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_existingRefreshToken);
        _authAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AuthAccountByMemberIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_authAccount);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
        _refreshTokenGenerator.Generate().Returns(("new-raw-token", "new-hash"));
        _tokenService.GenerateAccessToken(_member).Returns("new-access-token");
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static RefreshTokenCommand ValidCommand() => new() { RefreshToken = "valid-raw-token" };

    [Test]
    public async Task Handle_GivenActiveRefreshToken_WhenCalled_ThenReturnsNewAccessAndRefreshTokens()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("new-raw-token"));
            Assert.That(result.Member.Email, Is.EqualTo("host@example.com"));
        });
    }

    [Test]
    public async Task Handle_GivenActiveRefreshToken_WhenCalled_ThenRevokesOldTokenAndPersistsNewOne()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_existingRefreshToken.RevokedAt, Is.Not.Null);
            Assert.That(_existingRefreshToken.ReplacedByTokenId, Is.Not.Null);
        });
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshTokenAggregate>(t => t.TokenHash == "new-hash"), Arg.Any<CancellationToken>());
        await _refreshTokenRepository.Received(1).UpdateAsync(_existingRefreshToken, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownToken_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns((RefreshTokenAggregate?)null);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenRevokedToken_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        _existingRefreshToken.Revoke(DateTime.UtcNow);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAdminOwnedToken_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        var adminOwnedToken =
            RefreshTokenAggregate.Issue(Guid.NewGuid(), RefreshTokenOwnerType.Admin, "old-hash", DateTime.UtcNow);
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(adminOwnedToken);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
