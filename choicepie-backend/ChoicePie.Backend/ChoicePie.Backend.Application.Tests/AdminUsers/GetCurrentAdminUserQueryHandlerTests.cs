using ChoicePie.Backend.Application.AdminUsers.Contracts;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using ChoicePie.Backend.Application.AdminUsers.Queries;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.AdminUsers;

[TestFixture]
public class GetCurrentAdminUserQueryHandlerTests
{
    private IAdminUserQueryService _adminUserQueryService = null!;
    private ICurrentAdminUserService _currentAdminUserService = null!;
    private GetCurrentAdminUserQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _adminUserQueryService = Substitute.For<IAdminUserQueryService>();
        _currentAdminUserService = Substitute.For<ICurrentAdminUserService>();
        _sut = new GetCurrentAdminUserQueryHandler(_adminUserQueryService, _currentAdminUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedAdmin_WhenCalled_ThenReturnsAdminUserQueryServiceResult()
    {
        var adminUserId = Guid.NewGuid();
        var dto = new AdminUserDto(adminUserId, "admin@example.com", "Ops Name", "admin", false, DateTime.UtcNow);
        _currentAdminUserService.AdminUserId.Returns(adminUserId);
        _adminUserQueryService.GetByIdAsync(adminUserId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetCurrentAdminUserQuery(), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenNoCurrentAdminUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentAdminUserService.AdminUserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new GetCurrentAdminUserQuery(), CancellationToken.None));
    }
}
