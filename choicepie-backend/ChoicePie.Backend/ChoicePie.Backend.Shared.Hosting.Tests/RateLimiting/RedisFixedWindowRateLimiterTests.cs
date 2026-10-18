using System.Net;
using System.Threading.RateLimiting;
using ChoicePie.Backend.Shared.Hosting.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace ChoicePie.Backend.Shared.Hosting.Tests.RateLimiting;

/// <summary>兩條獨立的 Redis 連線代表兩台後端：計數必須共用，不能每台各算各的。</summary>
[TestFixture]
public class RedisFixedWindowRateLimiterTests
{
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    private ConnectionMultiplexer _nodeAConnection = null!;
    private ConnectionMultiplexer _nodeBConnection = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await _redis.StartAsync();
        _nodeAConnection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
        _nodeBConnection = await ConnectionMultiplexer.ConnectAsync(_redis.GetConnectionString());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _nodeAConnection.DisposeAsync();
        await _nodeBConnection.DisposeAsync();
        await _redis.DisposeAsync();
    }

    [Test]
    public async Task AcquireAsync_GivenRequestsBeyondPermitLimit_WhenAcquiring_ThenRejectsWithRetryAfter()
    {
        using var sut = CreateLimiter(_nodeAConnection, NewKey(), permitLimit: 2, windowSeconds: 60);

        using var first = await sut.AcquireAsync();
        using var second = await sut.AcquireAsync();
        using var third = await sut.AcquireAsync();

        Assert.That(first.IsAcquired, Is.True);
        Assert.That(second.IsAcquired, Is.True);
        Assert.That(third.IsAcquired, Is.False);
        Assert.That(third.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter), Is.True);
        Assert.That(retryAfter, Is.GreaterThan(TimeSpan.Zero).And.LessThanOrEqualTo(TimeSpan.FromSeconds(60)));
    }

    [Test]
    public async Task AcquireAsync_GivenSamePartitionOnTwoNodes_WhenAcquiring_ThenCountIsShared()
    {
        var key = NewKey();
        using var nodeA = CreateLimiter(_nodeAConnection, key, permitLimit: 3, windowSeconds: 60);
        using var nodeB = CreateLimiter(_nodeBConnection, key, permitLimit: 3, windowSeconds: 60);

        var leases = new List<RateLimitLease>();
        for (var i = 0; i < 4; i++)
        {
            leases.Add(await (i % 2 == 0 ? nodeA : nodeB).AcquireAsync());
        }

        Assert.That(leases.Select(l => l.IsAcquired), Is.EqualTo(new[] { true, true, true, false }));
        leases.ForEach(l => l.Dispose());
    }

    [Test]
    public async Task AcquireAsync_GivenConcurrentRequestsFromTwoNodes_WhenAcquiring_ThenNeverGrantsMoreThanLimit()
    {
        var key = NewKey();
        using var nodeA = CreateLimiter(_nodeAConnection, key, permitLimit: 10, windowSeconds: 60);
        using var nodeB = CreateLimiter(_nodeBConnection, key, permitLimit: 10, windowSeconds: 60);

        var leases = await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(i => (i % 2 == 0 ? nodeA : nodeB).AcquireAsync().AsTask()));

        Assert.That(leases.Count(l => l.IsAcquired), Is.EqualTo(10));
        foreach (var lease in leases) lease.Dispose();
    }

    [Test]
    public async Task AcquireAsync_GivenWindowElapsed_WhenAcquiring_ThenPermitsAgain()
    {
        using var sut = CreateLimiter(_nodeAConnection, NewKey(), permitLimit: 1, windowSeconds: 1);
        using var first = await sut.AcquireAsync();
        using var rejected = await sut.AcquireAsync();

        await Task.Delay(TimeSpan.FromMilliseconds(1200));
        using var afterWindow = await sut.AcquireAsync();

        Assert.That((first.IsAcquired, rejected.IsAcquired, afterWindow.IsAcquired), Is.EqualTo((true, false, true)));
    }

    [Test]
    public async Task AcquireAsync_GivenRedisUnreachable_WhenAcquiring_ThenFallsBackToLocalLimitInsteadOfThrowing()
    {
        var options = ConfigurationOptions.Parse("127.0.0.1:1");
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 100;
        await using var unreachable = await ConnectionMultiplexer.ConnectAsync(options);
        using var sut = CreateLimiter(unreachable, NewKey(), permitLimit: 2, windowSeconds: 60);

        using var first = await sut.AcquireAsync();
        using var second = await sut.AcquireAsync();
        using var third = await sut.AcquireAsync();

        Assert.That((first.IsAcquired, second.IsAcquired, third.IsAcquired), Is.EqualTo((true, true, false)));
    }

    /// <summary>ASP.NET Core 的 middleware 會先同步 AttemptAcquire 再 AcquireAsync，要確認整條路徑真的會回 429。</summary>
    [Test]
    public async Task RateLimitingMiddleware_GivenRedisLimiterAsGlobalLimiter_WhenLimitExceeded_ThenReturns429()
    {
        var key = NewKey();
        await using var app = await StartAppAsync(key, permitLimit: 2);
        var client = app.GetTestClient();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 3; i++)
        {
            statuses.Add((await client.GetAsync("/")).StatusCode);
        }

        Assert.That(statuses, Is.EqualTo(new[] { HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.TooManyRequests }));
    }

    private async Task<WebApplication> StartAppAsync(string key, int permitLimit)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                RateLimitPartition.Get(key, k => CreateLimiter(_nodeAConnection, k, permitLimit, 60)));
        });
        var app = builder.Build();
        app.UseRateLimiter();
        app.MapGet("/", () => "ok");
        await app.StartAsync();
        return app;
    }

    private static RedisFixedWindowRateLimiter CreateLimiter(
        IConnectionMultiplexer redis, string key, int permitLimit, int windowSeconds) =>
        new(key, new RateLimitRule { PermitLimit = permitLimit, WindowSeconds = windowSeconds }, redis);

    private static string NewKey() => $"test:{Guid.NewGuid():N}";
}
