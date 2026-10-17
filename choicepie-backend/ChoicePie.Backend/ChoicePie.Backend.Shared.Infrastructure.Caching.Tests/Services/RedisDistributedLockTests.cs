using ChoicePie.Backend.Shared.Infrastructure.Caching.Services;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace ChoicePie.Backend.Shared.Infrastructure.Caching.Tests.Services;

/// <summary>
/// 兩個獨立的 ConnectionMultiplexer 代表兩台後端，驗證鎖在節點之間互斥。
/// </summary>
[TestFixture]
public class RedisDistributedLockTests
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
    public async Task AcquireAsync_GivenLockHeldByNodeA_WhenNodeBAcquires_ThenWaitsUntilNodeAReleases()
    {
        var resource = NewResource();
        var nodeA = CreateLock(_nodeAConnection);
        var nodeB = CreateLock(_nodeBConnection);

        var nodeALease = await nodeA.AcquireAsync(resource);
        var nodeBAcquire = nodeB.AcquireAsync(resource);
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        Assert.That(nodeBAcquire.IsCompleted, Is.False, "node A 還沒釋放，node B 不能拿到鎖");

        await nodeALease.DisposeAsync();
        await using var nodeBLease = await nodeBAcquire.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task AcquireAsync_GivenConcurrentReadModifyWriteFromTwoNodes_WhenEachHoldsLock_ThenNoUpdateIsLost()
    {
        var resource = NewResource();
        var counterKey = new RedisKey($"counter:{resource}");
        var locks = new[] { CreateLock(_nodeAConnection), CreateLock(_nodeBConnection) };
        var db = _nodeAConnection.GetDatabase();

        // 故意拆成讀、等一下、再寫：沒有互斥就一定會互相覆蓋。
        var increments = Enumerable.Range(0, 20).Select(async i =>
        {
            await using var _ = await locks[i % 2].AcquireAsync(resource);
            var current = (int)await db.StringGetAsync(counterKey);
            await Task.Delay(5);
            await db.StringSetAsync(counterKey, current + 1);
        });
        await Task.WhenAll(increments);

        Assert.That((int)await db.StringGetAsync(counterKey), Is.EqualTo(20));
    }

    [Test]
    public async Task AcquireAsync_GivenLockNeverReleased_WhenWaitExceedsAcquireTimeout_ThenThrowsTimeoutException()
    {
        var resource = NewResource();
        await using var held = await CreateLock(_nodeAConnection).AcquireAsync(resource);
        var impatient = CreateLock(_nodeBConnection, acquireTimeout: TimeSpan.FromMilliseconds(300));

        Assert.ThrowsAsync<TimeoutException>(() => impatient.AcquireAsync(resource));
    }

    [Test]
    public async Task AcquireAsync_GivenHolderCrashedWithoutReleasing_WhenLeaseExpires_ThenOtherNodeAcquires()
    {
        var resource = NewResource();
        // 模擬持有者當機：拿到鎖後不釋放。
        _ = await CreateLock(_nodeAConnection, lease: TimeSpan.FromMilliseconds(500)).AcquireAsync(resource);

        await using var lease = await CreateLock(_nodeBConnection).AcquireAsync(resource)
            .WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task DisposeAsync_GivenLeaseAlreadyExpiredAndTakenByOtherNode_WhenReleased_ThenDoesNotReleaseOtherNodesLock()
    {
        var resource = NewResource();
        var staleLease = await CreateLock(_nodeAConnection, lease: TimeSpan.FromMilliseconds(300)).AcquireAsync(resource);
        await using var currentLease = await CreateLock(_nodeBConnection).AcquireAsync(resource)
            .WaitAsync(TimeSpan.FromSeconds(5));

        await staleLease.DisposeAsync();

        var third = CreateLock(_nodeAConnection, acquireTimeout: TimeSpan.FromMilliseconds(300));
        Assert.ThrowsAsync<TimeoutException>(() => third.AcquireAsync(resource),
            "過期的 lease 釋放時不可刪掉別人正持有的鎖");
    }

    [Test]
    public async Task TryAcquireAsync_GivenFreeResource_WhenCalled_ThenReturnsLease()
    {
        await using var lease = await CreateLock(_nodeAConnection).TryAcquireAsync(NewResource(), TimeSpan.FromSeconds(5));

        Assert.That(lease, Is.Not.Null);
    }

    [Test]
    public async Task TryAcquireAsync_GivenResourceHeldByOtherNode_WhenCalled_ThenReturnsNullWithoutWaiting()
    {
        var resource = NewResource();
        await using var held = await CreateLock(_nodeAConnection).TryAcquireAsync(resource, TimeSpan.FromSeconds(30));

        var attempt = CreateLock(_nodeBConnection).TryAcquireAsync(resource, TimeSpan.FromSeconds(30));

        Assert.That(await attempt.WaitAsync(TimeSpan.FromSeconds(1)), Is.Null);
    }

    [Test]
    public async Task TryAcquireAsync_GivenLeaseReleased_WhenOtherNodeTries_ThenSucceeds()
    {
        var resource = NewResource();
        var first = await CreateLock(_nodeAConnection).TryAcquireAsync(resource, TimeSpan.FromSeconds(30));
        await first!.DisposeAsync();

        await using var second = await CreateLock(_nodeBConnection).TryAcquireAsync(resource, TimeSpan.FromSeconds(30));

        Assert.That(second, Is.Not.Null);
    }

    [Test]
    public async Task TryAcquireAsync_GivenHolderCrashedWithoutReleasing_WhenCustomLeaseExpires_ThenOtherNodeSucceeds()
    {
        var resource = NewResource();
        _ = await CreateLock(_nodeAConnection).TryAcquireAsync(resource, TimeSpan.FromMilliseconds(300));

        await Task.Delay(TimeSpan.FromMilliseconds(500));
        await using var lease = await CreateLock(_nodeBConnection).TryAcquireAsync(resource, TimeSpan.FromSeconds(30));

        Assert.That(lease, Is.Not.Null);
    }

    private static RedisDistributedLock CreateLock(
        IConnectionMultiplexer connection, TimeSpan? lease = null, TimeSpan? acquireTimeout = null) =>
        new(connection, new RedisDistributedLockOptions
        {
            LeaseDuration = lease ?? TimeSpan.FromSeconds(15),
            AcquireTimeout = acquireTimeout ?? TimeSpan.FromSeconds(10)
        });

    private static string NewResource() => $"test:{Guid.NewGuid():N}";
}
