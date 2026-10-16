using System.Net;
using System.Security.Claims;
using ChoicePie.Backend.Shared.Hosting.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Hosting.Tests.RateLimiting;

[TestFixture]
public class RateLimitPartitionKeyResolverTests
{
    private static DefaultHttpContext CreateContext(string? sub, string? ip)
    {
        var context = new DefaultHttpContext();
        if (sub is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", sub)], "test"));
        }

        if (ip is not null)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        }

        return context;
    }

    [Test]
    public void Resolve_GivenAuthenticatedUser_WhenCalled_ThenReturnsUserKey()
    {
        var context = CreateContext("11111111-1111-1111-1111-111111111111", "10.0.0.1");

        var key = RateLimitPartitionKeyResolver.Resolve(context);

        Assert.That(key, Is.EqualTo("user:11111111-1111-1111-1111-111111111111"));
    }

    [Test]
    public void Resolve_GivenAnonymousRequest_WhenCalled_ThenReturnsIpKey()
    {
        var context = CreateContext(null, "10.0.0.1");

        Assert.That(RateLimitPartitionKeyResolver.Resolve(context), Is.EqualTo("ip:10.0.0.1"));
    }

    [Test]
    public void Resolve_GivenNoUserAndNoIp_WhenCalled_ThenReturnsUnknownKey()
    {
        var context = CreateContext(null, null);

        Assert.That(RateLimitPartitionKeyResolver.Resolve(context), Is.EqualTo("ip:unknown"));
    }

    [Test]
    public void ResolveIp_GivenAuthenticatedUser_WhenCalled_ThenStillReturnsIpKey()
    {
        var context = CreateContext("11111111-1111-1111-1111-111111111111", "10.0.0.1");

        Assert.That(RateLimitPartitionKeyResolver.ResolveIp(context), Is.EqualTo("ip:10.0.0.1"));
    }

    [Test]
    public void Resolve_GivenUnauthenticatedPrincipalWithSubClaim_WhenCalled_ThenFallsBackToIp()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "abc")]));
        context.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.2");

        Assert.That(RateLimitPartitionKeyResolver.Resolve(context), Is.EqualTo("ip:10.0.0.2"));
    }
}
