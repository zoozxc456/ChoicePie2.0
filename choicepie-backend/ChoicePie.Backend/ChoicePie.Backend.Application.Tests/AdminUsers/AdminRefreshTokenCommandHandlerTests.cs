using ChoicePie.Backend.Application.AdminUsers.Commands;
using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class AdminRefreshTokenCommandHandlerTests
{
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IAdminAuthAccountRepository _adminAuthAccountRepository = null!;
    private IAdminUserRepository _adminUserRepository = null!;
    private IAdminTokenService _tokenService = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private AdminRefreshTokenCommandHandler _sut = null!;
    private AdminUser _adminUser = null!;
    private AdminAuthAccount _adminAuthAccount = null!;
    private RefreshTokenAggregate _existingRefreshToken = null!;

    [SetUp]
    public void SetUp()
    {
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _adminAuthAccountRepository = Substitute.For<IAdminAuthAccountRepository>();
        _adminUserRepository = Substitute.For<IAdminUserRepository>();
        _tokenService = Substitute.For<IAdminTokenService>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new AdminRefreshTokenCommandHandler(_refreshTokenRepository, _adminAuthAccountRepository,
            _adminUserRepository, _tokenService, _refreshTokenGenerator, _unitOfWork, _timeProvider);

        _adminUser = AdminUser.Create("Ops Name", AdminRole.Staff);
        _adminAuthAccount = AdminAuthAccount.Create(
            Email.Create("admin@example.com"), HashedPassword.Create("hashed-password", "salt"), _adminUser.Id);
        _existingRefreshToken =
            RefreshTokenAggregate.Issue(_adminUser.Id, RefreshTokenOwnerType.Admin, "old-hash", DateTime.UtcNow);

        _refreshTokenGenerator.Hash("valid-raw-token").Returns("old-hash");
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_existingRefreshToken);
        _adminAuthAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByAdminUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_adminAuthAccount);
        _adminUserRepository.GetByIdAsync(_adminUser.Id, Arg.Any<CancellationToken>()).Returns(_adminUser);
        _refreshTokenGenerator.Generate().Returns(("new-raw-token", "new-hash"));
        _tokenService.GenerateAccessToken(_adminUser).Returns("new-access-token");
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static AdminRefreshTokenCommand ValidCommand() => new() { RefreshToken = "valid-raw-token" };

    [Test]
    public async Task Handle_GivenActiveRefreshToken_WhenCalled_ThenReturnsNewAccessAndRefreshTokens()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.EqualTo("new-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("new-raw-token"));
            Assert.That(result.AdminUser.Email, Is.EqualTo("admin@example.com"));
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
    public void Handle_GivenMemberOwnedToken_WhenCalled_ThenThrowsInvalidRefreshTokenException()
    {
        var memberOwnedToken =
            RefreshTokenAggregate.Issue(Guid.NewGuid(), RefreshTokenOwnerType.Member, "old-hash", DateTime.UtcNow);
        _refreshTokenRepository
            .FirstOrDefaultAsync(Arg.Any<RefreshTokenByTokenHashSpecification>(), Arg.Any<CancellationToken>())
            .Returns(memberOwnedToken);

        Assert.ThrowsAsync<InvalidRefreshTokenException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
