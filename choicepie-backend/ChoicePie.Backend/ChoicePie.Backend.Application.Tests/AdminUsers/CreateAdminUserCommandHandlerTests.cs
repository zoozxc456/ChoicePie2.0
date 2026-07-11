using ChoicePie.Backend.Application.AdminUsers.Commands;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class CreateAdminUserCommandHandlerTests
{
    private IAdminUserRepository _adminUserRepository = null!;
    private IAdminAuthAccountRepository _adminAuthAccountRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IUnitOfWork _unitOfWork = null!;
    private ICurrentAdminUserService _currentAdminUserService = null!;
    private CreateAdminUserCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _adminUserRepository = Substitute.For<IAdminUserRepository>();
        _adminAuthAccountRepository = Substitute.For<IAdminAuthAccountRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _currentAdminUserService = Substitute.For<ICurrentAdminUserService>();
        _sut = new CreateAdminUserCommandHandler(
            _adminUserRepository, _adminAuthAccountRepository, _passwordHasher, _unitOfWork, _currentAdminUserService);

        _currentAdminUserService.AdminUserId.Returns(Guid.NewGuid());
        _adminAuthAccountRepository
            .ExistsAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(HashedPassword.Create("hashed-password", "salt"));
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    private static CreateAdminUserCommand ValidCommand() => new()
    {
        Email = "admin@example.com",
        Name = "Ops Name",
        Password = "password123",
        ConfirmPassword = "password123",
        Role = "staff"
    };

    [Test]
    public async Task Handle_GivenAuthenticatedCaller_WhenCalled_ThenPersistsAdminUserAndAdminAuthAccount()
    {
        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _adminUserRepository.Received(1).AddAsync(
            Arg.Is<AdminUser>(a => a.Name == "Ops Name" && a.Role == AdminRole.Staff),
            Arg.Any<CancellationToken>());
        await _adminAuthAccountRepository.Received(1).AddAsync(
            Arg.Is<AdminAuthAccount>(a =>
                a.Email.Value == "admin@example.com" &&
                a.OriginalPassword == HashedPassword.Create("hashed-password", "salt")),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_GivenAuthenticatedCaller_WhenCalled_ThenAdminAuthAccountLinksToPersistedAdminUser()
    {
        AdminUser? capturedAdminUser = null;
        AdminAuthAccount? capturedAdminAuthAccount = null;
        _ = _adminUserRepository.AddAsync(Arg.Do<AdminUser>(a => capturedAdminUser = a), Arg.Any<CancellationToken>());
        _ = _adminAuthAccountRepository.AddAsync(Arg.Do<AdminAuthAccount>(a => capturedAdminAuthAccount = a),
            Arg.Any<CancellationToken>());

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(capturedAdminAuthAccount!.AdminUserId, Is.EqualTo(capturedAdminUser!.Id));
    }

    [Test]
    public async Task Handle_GivenAuthenticatedCaller_WhenCalled_ThenReturnsAdminUserDto()
    {
        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Email, Is.EqualTo("admin@example.com"));
            Assert.That(result.Name, Is.EqualTo("Ops Name"));
            Assert.That(result.Role, Is.EqualTo("staff"));
            Assert.That(result.IsVerified, Is.False);
        });
    }

    [Test]
    public void Handle_GivenNoCurrentAdminUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentAdminUserService.AdminUserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenEmailAlreadyRegistered_WhenCalled_ThenThrowsEmailAlreadyRegisteredException()
    {
        _adminAuthAccountRepository
            .ExistsAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        Assert.ThrowsAsync<EmailAlreadyRegisteredException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenEmailAlreadyRegistered_WhenCalled_ThenDoesNotPersistAdminUserOrAdminAuthAccount()
    {
        _adminAuthAccountRepository
            .ExistsAsync(Arg.Any<AdminAuthAccountByEmailSpecification>(), Arg.Any<CancellationToken>())
            .Returns(true);

        try
        {
            await _sut.Handle(ValidCommand(), CancellationToken.None);
        }
        catch (EmailAlreadyRegisteredException)
        {
            // expected
        }

        await _adminUserRepository.DidNotReceive().AddAsync(Arg.Any<AdminUser>(), Arg.Any<CancellationToken>());
        await _adminAuthAccountRepository.DidNotReceive()
            .AddAsync(Arg.Any<AdminAuthAccount>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenUnknownRole_WhenCalled_ThenThrowsInvalidAdminRoleException()
    {
        var command = new CreateAdminUserCommand
        {
            Email = "admin@example.com",
            Name = "Ops Name",
            Password = "password123",
            ConfirmPassword = "password123",
            Role = "guest"
        };

        Assert.ThrowsAsync<InvalidAdminRoleException>(() => _sut.Handle(command, CancellationToken.None));
    }
}