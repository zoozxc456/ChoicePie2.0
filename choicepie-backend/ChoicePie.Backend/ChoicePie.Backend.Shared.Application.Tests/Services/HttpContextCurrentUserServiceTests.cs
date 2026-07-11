using System.Security.Claims;
using ChoicePie.Backend.Shared.Hosting.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Application.Tests.Services;

[TestFixture]
public class HttpContextCurrentUserServiceTests
{
    [Test]
    public void UserId_GivenHttpContextWithSubAndMemberRoleClaim_WhenRead_ThenReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", userId.ToString()),
            new Claim("role", "member")
        ])));
        var sut = new HttpContextCurrentUserService(accessor);

        Assert.That(sut.UserId, Is.EqualTo(userId));
    }

    [Test]
    public void UserId_GivenHttpContextWithoutSubClaim_WhenRead_ThenReturnsNull()
    {
        var accessor = CreateAccessor(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("role", "member")
        ])));
        var sut = new HttpContextCurrentUserService(accessor);

        Assert.That(sut.UserId, Is.Null);
    }

    [Test]
    public void UserId_GivenHttpContextWithAdminRoleClaim_WhenRead_ThenReturnsNull()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("sub", userId.ToString()),
            new Claim("role", "admin")
        ])));
        var sut = new HttpContextCurrentUserService(accessor);

        Assert.That(sut.UserId, Is.Null);
    }

    [Test]
    public void UserId_GivenNoHttpContext_WhenRead_ThenReturnsNull()
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext?)null);
        var sut = new HttpContextCurrentUserService(accessor);

        Assert.That(sut.UserId, Is.Null);
    }

    private static IHttpContextAccessor CreateAccessor(ClaimsPrincipal user)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(new DefaultHttpContext { User = user });
        return accessor;
    }
}
