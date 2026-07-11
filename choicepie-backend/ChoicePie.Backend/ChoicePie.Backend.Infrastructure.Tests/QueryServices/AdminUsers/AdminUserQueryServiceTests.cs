using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Infrastructure.QueryServices.AdminUsers;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.AdminUsers;

[TestFixture]
public class AdminUserQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private AdminUserQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new AdminUserQueryService(_readRepository);
    }

    [Test]
    public async Task GetByIdAsync_GivenAdminUserWithMatchingAuthAccount_WhenCalled_ThenReturnsAdminUserDto()
    {
        var adminUser = AdminUser.Create("Ops Name", AdminRole.Admin);
        var adminAuthAccount = AdminAuthAccount.Create(Email.Create("admin@example.com"), "hashed-password", adminUser.Id);
        _readRepository.Query<AdminUser>().Returns(new List<AdminUser> { adminUser }.AsQueryable());
        _readRepository.Query<AdminAuthAccount>().Returns(new List<AdminAuthAccount> { adminAuthAccount }.AsQueryable());

        var result = await _sut.GetByIdAsync(adminUser.Id, CancellationToken.None);

        Assert.That(result.Email, Is.EqualTo("admin@example.com"));
    }

    [Test]
    public void GetByIdAsync_GivenUnknownAdminUserId_WhenCalled_ThenThrowsAdminUserNotFoundException()
    {
        _readRepository.Query<AdminUser>().Returns(new List<AdminUser>().AsQueryable());

        Assert.ThrowsAsync<AdminUserNotFoundException>(() => _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Test]
    public void GetByIdAsync_GivenAdminUserWithNoMatchingAuthAccount_WhenCalled_ThenThrowsAdminAuthAccountNotFoundException()
    {
        var adminUser = AdminUser.Create("Ops Name", AdminRole.Admin);
        _readRepository.Query<AdminUser>().Returns(new List<AdminUser> { adminUser }.AsQueryable());
        _readRepository.Query<AdminAuthAccount>().Returns(new List<AdminAuthAccount>().AsQueryable());

        Assert.ThrowsAsync<AdminAuthAccountNotFoundException>(() => _sut.GetByIdAsync(adminUser.Id, CancellationToken.None));
    }
}
