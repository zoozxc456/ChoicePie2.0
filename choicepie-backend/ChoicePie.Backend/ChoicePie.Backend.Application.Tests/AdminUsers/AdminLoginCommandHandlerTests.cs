using ChoicePie.Backend.Application.AdminUsers.Commands;
using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class AdminLoginCommandHandlerTests
{
    private IAdminAuthAccountRepository _adminAuthAccountRepository = null!;
    private IAdminUserRepository _adminUserRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IAdminTokenService _tokenService = null!;
    private AdminLoginCommandHandler _sut = null!;
    private AdminUser _adminUser = null!;
    private AdminAuthAccount _adminAuthAccount = null!;

    [SetUp]
    public void SetUp()
    {
        _adminAuthAccountRepository = Substitute.For<IAdminAuthAccountRepository>();
        _adminUserRepository = Substitute.For<IAdminUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _tokenService = Substitute.For<IAdminTokenService>();
        _sut = new AdminLoginCommandHandler(_adminAuthAccountRepository, _adminUserRepository, _passwordHasher, _tokenService);

        _adminUser = AdminUser.Create("Ops Name", AdminRole.Staff);
        _adminAuthAccount = AdminAuthAccount.Create(Email.Create("admin@example.com"), "hashed-password", _adminUser.Id);
        _adminAuthAccountRepository.FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(_adminAuthAccount);
        _adminUserRepository.GetByIdAsync(_adminUser.Id, Arg.Any<CancellationToken>()).Returns(_adminUser);
    }

    private static AdminLoginCommand ValidCommand() => new() { Email = "admin@example.com", Password = "correct-password" };

    [Test]
    public async Task Handle_GivenValidCredentials_WhenCalled_ThenReturnsAdminUserAndToken()
    {
        _passwordHasher.Verify("correct-password", "hashed-password").Returns(true);
        _tokenService.GenerateToken(_adminUser).Returns("jwt-token");

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Token, Is.EqualTo("jwt-token"));
            Assert.That(result.AdminUser.Email, Is.EqualTo("admin@example.com"));
        });
    }

    [Test]
    public void Handle_GivenUnknownEmail_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _adminAuthAccountRepository.FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AdminAuthAccount?)null);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenWrongPassword_WhenCalled_ThenThrowsInvalidCredentialsException()
    {
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }
}
