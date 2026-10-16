using ChoicePie.Backend.Shared.Hosting.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ChoicePie.Backend.Shared.Hosting.Tests.HealthChecks;

[TestFixture]
public class DistributedCacheHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_GivenCacheResponds_WhenCalled_ThenReturnsHealthy()
    {
        var cache = Substitute.For<IDistributedCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((byte[]?)null);
        var sut = new DistributedCacheHealthCheck(cache);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [Test]
    public async Task CheckHealthAsync_GivenCacheThrows_WhenCalled_ThenReturnsUnhealthyWithException()
    {
        var cache = Substitute.For<IDistributedCache>();
        var boom = new InvalidOperationException("redis down");
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(boom);
        var sut = new DistributedCacheHealthCheck(cache);

        var result = await sut.CheckHealthAsync(new HealthCheckContext());

        Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
        Assert.That(result.Exception, Is.SameAs(boom));
    }
}
