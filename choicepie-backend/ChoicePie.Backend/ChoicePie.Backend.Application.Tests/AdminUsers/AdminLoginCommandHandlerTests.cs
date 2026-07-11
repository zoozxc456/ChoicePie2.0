using ChoicePie.Backend.Application.AdminUsers.Commands;
using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class AdminLoginCommandHandlerTests
{
    private IAdminAuthAccountRepository _adminAuthAccountRepository = null!;
    private IAdminUserRepository _adminUserRepository = null!;
    private IRefreshTokenRepository _refreshTokenRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IAdminTokenService _tokenService = null!;
    private IRefreshTokenGenerator _refreshTokenGenerator = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminLoginCommandHandler _sut = null!;
    private AdminUser _adminUser = null!;
    private AdminAuthAccount _adminAuthAccount = null!;

    [SetUp]
    public void SetUp()
    {
        _adminAuthAccountRepository = Substitute.For<IAdminAuthAccountRepository>();
        _adminUserRepository = Substitute.For<IAdminUserRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<IAdminTokenService>();
        _refreshTokenGenerator = Substitute.For<IRefreshTokenGenerator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new AdminLoginCommandHandler(_adminAuthAccountRepository, _adminUserRepository,
            _refreshTokenRepository, _passwordHasher, _tokenService, _refreshTokenGenerator, _unitOfWork);

        _adminUser = AdminUser.Create("Ops Name", AdminRole.Staff);
        _adminAuthAccount = AdminAuthAccount.Create(
            Email.Create("admin@example.com"), HashedPassword.Create("hashed-password", "salt"), _adminUser.Id);
        _adminAuthAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_adminAuthAccount);
        _adminUserRepository.GetByIdAsync(_adminUser.Id, Arg.Any<CancellationToken>()).Returns(_adminUser);
        _refreshTokenGenerator.Generate().Returns(("raw-refresh-token", "refresh-token-hash"));
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static AdminLoginCommand ValidCommand() =>
        new() { Email = "admin@example.com", Password = "correct-password" };

    [Test]
    public async Task Handle_GivenValidCredentials_WhenCalled_ThenReturnsAdminUserAndTokens()
    {
        _passwordHasher.Verify("correct-password", HashedPassword.Create("hashed-password", "salt")).Returns(true);
        _tokenService.GenerateAccessToken(_adminUser).Returns("jwt-access-token");

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.AccessToken, Is.EqualTo("jwt-access-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("raw-refresh-token"));
            Assert.That(result.AdminUser.Email, Is.EqualTo("admin@example.com"));
        });
    }

    [Test]
    public async Task Handle_GivenValidCredentials_WhenCalled_ThenPersistsRefreshTokenForAdminUser()
    {
        _passwordHasher.Verify("correct-password", HashedPassword.Create("hashed-password", "salt")).Returns(true);
        _tokenService.GenerateAccessToken(_adminUser).Returns("jwt-access-token");

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshTokenAggregate>(t => t.OwnerId == _adminUser.Id && t.TokenHash == "refresh-token-hash"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownEmail_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _adminAuthAccountRepository
            .FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AdminAuthAccount?)null);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenWrongPassword_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<HashedPassword>()).Returns(false);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
