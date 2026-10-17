using ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.Outbox;

/// <summary>
/// 認領／完成／失敗都是直接打資料庫的單一陳述式；多台後端同時跑 processor，
/// 同一筆訊息在租約期間只能被一台拿到。
/// </summary>
public sealed class OutboxStoreTests
{
    private static readonly TimeSpan Lease = TimeSpan.FromMinutes(5);
    private readonly DateTime _now = new(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);
    private PersistenceTestFixture _fixture = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new PersistenceTestFixture();
        await _fixture.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _fixture.DisposeAsync();

    [SetUp]
    public async Task SetUp()
    {
        await using var context = _fixture.CreateContext();
        await context.Set<OutboxMessage>().ExecuteDeleteAsync();
    }

    [Test]
    public async Task ClaimBatchAsync_GivenDueMessage_WhenClaimedTwiceWithinLease_ThenOnlyTheFirstClaimGetsIt()
    {
        var id = await SeedMessageAsync();

        var first = await ClaimAsync(_now);
        var second = await ClaimAsync(_now.AddMinutes(1));

        Assert.That(first.Select(m => m.Id), Is.EqualTo(new[] { id }));
        Assert.That(second, Is.Empty);
    }

    // 處理到一半整台掛掉：租約到期後別台要能接手，事件才不會卡死。
    [Test]
    public async Task ClaimBatchAsync_GivenClaimedMessageWhoseLeaseExpired_WhenClaimedAgain_ThenItIsReturned()
    {
        var id = await SeedMessageAsync();
        await ClaimAsync(_now);

        var reclaimed = await ClaimAsync(_now.Add(Lease).AddSeconds(1));

        Assert.That(reclaimed.Select(m => m.Id), Is.EqualTo(new[] { id }));
    }

    [Test]
    public async Task CompleteAsync_GivenClaimedMessage_WhenCompleted_ThenRowIsDeleted()
    {
        var id = await SeedMessageAsync();
        await ClaimAsync(_now);

        await using var context = _fixture.CreateContext();
        await CreateStore(context, _now).CompleteAsync(id, CancellationToken.None);

        Assert.That(await context.Set<OutboxMessage>().AnyAsync(m => m.Id == id), Is.False);
    }

    [Test]
    public async Task FailAsync_GivenFirstFailure_WhenFailed_ThenRetriedOnlyAfterBackoffAndCompletedHandlersAreKept()
    {
        var id = await SeedMessageAsync();
        await ClaimAsync(_now);

        await using (var context = _fixture.CreateContext())
        {
            await CreateStore(context, _now).FailAsync(id, ["HandlerA"], "boom", CancellationToken.None);
        }

        Assert.That(await ClaimAsync(_now.AddSeconds(1)), Is.Empty);
        var retried = await ClaimAsync(_now.Add(OutboxStore.BackoffFor(attempt: 1)));
        Assert.That(retried.Single().Id, Is.EqualTo(id));
        Assert.That(retried.Single().AttemptCount, Is.EqualTo(1));
        Assert.That(retried.Single().CompletedHandlers, Is.EqualTo(new[] { "HandlerA" }));
        Assert.That(retried.Single().LastError, Is.EqualTo("boom"));
    }

    [Test]
    public async Task FailAsync_GivenFinalAttempt_WhenFailed_ThenMessageIsDeadLetteredAndNeverClaimedAgain()
    {
        var id = await SeedMessageAsync();
        var clock = _now;
        for (var attempt = 1; attempt <= OutboxStore.MaxAttempts; attempt++)
        {
            Assert.That(await ClaimAsync(clock), Has.Count.EqualTo(1), $"attempt {attempt} should be claimable");
            await using var context = _fixture.CreateContext();
            await CreateStore(context, clock).FailAsync(id, [], "still failing", CancellationToken.None);
            clock = clock.Add(OutboxStore.BackoffFor(attempt));
        }

        Assert.That(await ClaimAsync(clock.AddDays(1)), Is.Empty);
        await using var verify = _fixture.CreateContext();
        var message = await verify.Set<OutboxMessage>().SingleAsync(m => m.Id == id);
        Assert.That(message.DeadLetteredAt, Is.Not.Null);
    }

    private async Task<Guid> SeedMessageAsync()
    {
        var domainEvent = new TestSecretIssuedDomainEvent(Guid.NewGuid(), "token") { OccurredOn = _now };
        var (type, payload) = _fixture.OutboxSerializer.Serialize(domainEvent);
        await using var context = _fixture.CreateContext();
        await context.AddAsync(OutboxMessage.Create(domainEvent.EventId, type, payload, domainEvent.OccurredOn));
        await context.SaveChangesAsync();
        return domainEvent.EventId;
    }

    private async Task<IReadOnlyList<OutboxMessage>> ClaimAsync(DateTime now)
    {
        await using var context = _fixture.CreateContext();
        return await CreateStore(context, now).ClaimBatchAsync(batchSize: 10, Lease, CancellationToken.None);
    }

    private static OutboxStore CreateStore(TestDbContext context, DateTime now)
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(new DateTimeOffset(now));
        return new OutboxStore(context, timeProvider);
    }
}
