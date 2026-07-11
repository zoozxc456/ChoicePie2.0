using System.Security.Claims;
using ChoicePie.Backend.Shared.Hosting.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Application.Tests.Services;

[TestFixture]
public class HttpContextCurrentAdminUserServiceTests
{
    [Test]
    public void AdminUserId_GivenHttpContextWithSubAndAdminRoleClaim_WhenRead_ThenReturnsParsedGuid()
    {
        var adminUserId = Guid.NewGuid();
        var accessor = CreateAccessor(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", adminUserId.ToString()),
            new Claim("role", "admin")
        ])));
        var sut = new HttpContextCurrentAdminUserService(accessor);

        Assert.That(sut.AdminUserId, Is.EqualTo(adminUserId));
    }

    [Test]
    public void AdminUserId_GivenHttpContextWithoutSubClaim_WhenRead_ThenReturnsNull()
    {
        var accessor = CreateAccessor(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("role", "admin")
        ])));
        var sut = new HttpContextCurrentAdminUserService(accessor);

        Assert.That(sut.AdminUserId, Is.Null);
    }

    [Test]
    public void AdminUserId_GivenHttpContextWithMemberRoleClaim_WhenRead_ThenReturnsNull()
    {
        var adminUserId = Guid.NewGuid();
        var accessor = CreateAccessor(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", adminUserId.ToString()),
            new Claim("role", "member")
        ])));
        var sut = new HttpContextCurrentAdminUserService(accessor);

        Assert.That(sut.AdminUserId, Is.Null);
    }

    [Test]
    public void AdminUserId_GivenNoHttpContext_WhenRead_ThenReturnsNull()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var sut = new HttpContextCurrentAdminUserService(accessor);

        Assert.That(sut.AdminUserId, Is.Null);
    }

    private static IHttpContextAccessor CreateAccessor(ClaimsPrincipal user)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(new DefaultHttpContext { User = user });
        return accessor;
    }
}
