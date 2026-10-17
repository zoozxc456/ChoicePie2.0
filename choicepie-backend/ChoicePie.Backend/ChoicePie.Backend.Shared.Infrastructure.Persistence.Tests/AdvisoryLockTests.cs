using ChoicePie.Backend.Shared.Infrastructure.Persistence.Extensions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests;

/// <summary>多台同時啟動時用來讓 migration / seed 排隊；兩個 DbContext 代表兩台後端。</summary>
public sealed class AdvisoryLockTests
{
    private PersistenceTestFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new PersistenceTestFixture();
        await _fixture.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _fixture.DisposeAsync();

    [Test]
    public async Task AcquireAdvisoryLockAsync_GivenLockHeldByOtherNode_WhenAcquiring_ThenWaitsUntilReleased()
    {
        const long lockKey = 1001;
        await using var nodeA = _fixture.CreateContext();
        await using var nodeB = _fixture.CreateContext();

        var nodeALock = await nodeA.Database.AcquireAdvisoryLockAsync(lockKey);
        var nodeBAcquire = nodeB.Database.AcquireAdvisoryLockAsync(lockKey);
        await Task.Delay(TimeSpan.FromMilliseconds(300));

        Assert.That(nodeBAcquire.IsCompleted, Is.False, "node A 還沒釋放，node B 必須等待");

        await nodeALock.DisposeAsync();
        await using var nodeBLock = await nodeBAcquire.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task AcquireAdvisoryLockAsync_GivenDifferentKeys_WhenAcquiring_ThenDoesNotBlock()
    {
        await using var nodeA = _fixture.CreateContext();
        await using var nodeB = _fixture.CreateContext();

        await using var first = await nodeA.Database.AcquireAdvisoryLockAsync(2001);
        await using var second = await nodeB.Database.AcquireAdvisoryLockAsync(2002).WaitAsync(TimeSpan.FromSeconds(5));
    }
}
