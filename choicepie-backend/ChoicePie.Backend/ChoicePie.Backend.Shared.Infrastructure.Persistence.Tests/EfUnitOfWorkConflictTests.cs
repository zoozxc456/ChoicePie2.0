using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;
using ChoicePie.Backend.Shared.Kernel.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests;

/// <summary>
/// 兩個 DbContext 代表兩個同時進來的請求。資料庫層的衝突要轉成 Application 層看得懂、
/// 預設回 409 的 DomainException，而不是讓 EF / Npgsql 的例外直接變成 500。
/// </summary>
public sealed class EfUnitOfWorkConflictTests
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
    public async Task SaveChangesAsync_GivenConcurrentInsertsOfSameUniqueKey_WhenSecondSaves_ThenThrowsUniqueConstraintViolation()
    {
        var code = Guid.NewGuid().ToString("N")[..20];
        await using var first = _fixture.CreateContext();
        await using var second = _fixture.CreateContext();
        await first.AddAsync(TestGadget.Create(code));
        await second.AddAsync(TestGadget.Create(code));

        await CreateUnitOfWork(first).SaveChangesAsync();
        var ex = Assert.ThrowsAsync<UniqueConstraintViolationException>(() => CreateUnitOfWork(second).SaveChangesAsync());

        Assert.That(ex!.ConstraintName, Does.Contain("code"));
        Assert.That(ex.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
    }

    [Test]
    public async Task SaveChangesAsync_GivenRowChangedSinceLoaded_WhenSaving_ThenThrowsConcurrencyConflict()
    {
        var gadget = TestGadget.Create(Guid.NewGuid().ToString("N")[..20]);
        await using (var seed = _fixture.CreateContext())
        {
            await seed.AddAsync(gadget);
            await CreateUnitOfWork(seed).SaveChangesAsync();
        }

        await using var first = _fixture.CreateContext();
        await using var second = _fixture.CreateContext();
        var firstCopy = await first.Set<TestGadget>().SingleAsync(g => g.Id == gadget.Id);
        var secondCopy = await second.Set<TestGadget>().SingleAsync(g => g.Id == gadget.Id);
        firstCopy.Relabel("first");
        secondCopy.Relabel("second");

        await CreateUnitOfWork(first).SaveChangesAsync();
        var ex = Assert.ThrowsAsync<ConcurrencyConflictException>(() => CreateUnitOfWork(second).SaveChangesAsync());

        Assert.That(ex!.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Conflict));
    }

    [Test]
    public async Task SaveChangesAsync_GivenRowUnchangedSinceLoaded_WhenSavingTwiceSequentially_ThenSucceeds()
    {
        var gadget = TestGadget.Create(Guid.NewGuid().ToString("N")[..20]);
        await using var context = _fixture.CreateContext();
        await context.AddAsync(gadget);
        var unitOfWork = CreateUnitOfWork(context);
        await unitOfWork.SaveChangesAsync();

        gadget.Relabel("once");
        await unitOfWork.SaveChangesAsync();
        gadget.Relabel("twice");

        Assert.DoesNotThrowAsync(() => unitOfWork.SaveChangesAsync());
    }

    private Repositories.EfUnitOfWork<TestDbContext> CreateUnitOfWork(TestDbContext context) =>
        _fixture.CreateUnitOfWork(context, Substitute.For<IPublisher>());
}
