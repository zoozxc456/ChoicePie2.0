using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Infrastructure.Persistence.Seeding;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.Persistence.Seeding;

[TestFixture]
public class AdminUserSeederTests
{
    private IAdminUserRepository _adminUserRepository = null!;
    private IAdminAuthAccountRepository _adminAuthAccountRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IUnitOfWork _unitOfWork = null!;
    private AdminUserSeeder _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _adminUserRepository = Substitute.For<IAdminUserRepository>();
        _adminAuthAccountRepository = Substitute.For<IAdminAuthAccountRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _adminUserRepository.GetOneAsync(Arg.Any<CancellationToken>()).Returns((AdminUser?)null);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(HashedPassword.Create("hashed-password", "salt"));

        var settings = Options.Create(new AdminBootstrapSettings
        {
            Email = "admin@choicepie.local",
            Name = "System Admin",
            Password = "ChangeMe123!"
        });

        _sut = new AdminUserSeeder(
            _adminUserRepository, _adminAuthAccountRepository, _passwordHasher, _unitOfWork, settings);
    }

    [TearDown]
    public void TearDown()
    {
        _unitOfWork.Dispose();
    }

    [Test]
    public async Task SeedAsync_GivenNoExistingAdmin_WhenCalled_ThenCreatesAdminUserAndAdminAuthAccount()
    {
        await _sut.SeedAsync();

        await _adminUserRepository.Received(1).AddAsync(
            Arg.Is<AdminUser>(a => a.Name == "System Admin" && a.Role == AdminRole.SystemAdmin),
            Arg.Any<CancellationToken>());
        await _adminAuthAccountRepository.Received(1).AddAsync(
            Arg.Is<AdminAuthAccount>(a => a.Email.Value == "admin@choicepie.local"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SeedAsync_GivenExistingAdmin_WhenCalled_ThenDoesNotCreateAnotherAdmin()
    {
        _adminUserRepository.GetOneAsync(Arg.Any<CancellationToken>())
            .Returns(AdminUser.Create("Existing Admin", AdminRole.Admin));

        await _sut.SeedAsync();

        await _adminUserRepository.DidNotReceive().AddAsync(Arg.Any<AdminUser>(), Arg.Any<CancellationToken>());
        await _adminAuthAccountRepository.DidNotReceive()
            .AddAsync(Arg.Any<AdminAuthAccount>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task SeedAsync_GivenNoBootstrapCredentialsConfigured_WhenCalled_ThenDoesNotCreateAdmin()
    {
        var settings = Options.Create(new AdminBootstrapSettings());
        var sut = new AdminUserSeeder(
            _adminUserRepository, _adminAuthAccountRepository, _passwordHasher, _unitOfWork, settings);

        await sut.SeedAsync();

        await _adminUserRepository.DidNotReceive().AddAsync(Arg.Any<AdminUser>(), Arg.Any<CancellationToken>());
    }
}
