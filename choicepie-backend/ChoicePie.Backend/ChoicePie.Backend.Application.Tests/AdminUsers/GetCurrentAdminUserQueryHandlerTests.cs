using ChoicePie.Backend.Application.AdminUsers.Queries;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Specifications;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class GetCurrentAdminUserQueryHandlerTests
{
    private IAdminUserRepository _adminUserRepository = null!;
    private IAdminAuthAccountRepository _adminAuthAccountRepository = null!;
    private ICurrentAdminUserService _currentAdminUserService = null!;
    private GetCurrentAdminUserQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _adminUserRepository = Substitute.For<IAdminUserRepository>();
        _adminAuthAccountRepository = Substitute.For<IAdminAuthAccountRepository>();
        _currentAdminUserService = Substitute.For<ICurrentAdminUserService>();
        _sut = new GetCurrentAdminUserQueryHandler(_adminUserRepository, _adminAuthAccountRepository, _currentAdminUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedAdminWithMatchingRecord_WhenCalled_ThenReturnsAdminUserDto()
    {
        var adminUser = AdminUser.Create("Ops Name", AdminRole.Admin);
        var adminAuthAccount = AdminAuthAccount.Create(Email.Create("admin@example.com"), "hashed-password", adminUser.Id);
        _currentAdminUserService.AdminUserId.Returns(adminUser.Id);
        _adminUserRepository.GetByIdAsync(adminUser.Id, Arg.Any<CancellationToken>()).Returns(adminUser);
        _adminAuthAccountRepository.FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByAdminUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(adminAuthAccount);

        var result = await _sut.Handle(new GetCurrentAdminUserQuery(), CancellationToken.None);

        Assert.That(result.Email, Is.EqualTo("admin@example.com"));
    }

    [Test]
    public void Handle_GivenNoCurrentAdminUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentAdminUserService.AdminUserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new GetCurrentAdminUserQuery(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenCurrentAdminUserIdHasNoMatchingRecord_WhenCalled_ThenThrowsAdminUserNotFoundException()
    {
        var adminUserId = Guid.NewGuid();
        _currentAdminUserService.AdminUserId.Returns(adminUserId);
        _adminUserRepository.GetByIdAsync(adminUserId, Arg.Any<CancellationToken>()).Returns((AdminUser?)null);

        Assert.ThrowsAsync<AdminUserNotFoundException>(() => _sut.Handle(new GetCurrentAdminUserQuery(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenAdminUserWithNoMatchingAuthAccount_WhenCalled_ThenThrowsAdminAuthAccountNotFoundException()
    {
        var adminUser = AdminUser.Create("Ops Name", AdminRole.Admin);
        _currentAdminUserService.AdminUserId.Returns(adminUser.Id);
        _adminUserRepository.GetByIdAsync(adminUser.Id, Arg.Any<CancellationToken>()).Returns(adminUser);
        _adminAuthAccountRepository.FirstOrDefaultAsync(Arg.Any<AdminAuthAccountByAdminUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((AdminAuthAccount?)null);

        Assert.ThrowsAsync<AdminAuthAccountNotFoundException>(() => _sut.Handle(new GetCurrentAdminUserQuery(), CancellationToken.None));
    }
}
