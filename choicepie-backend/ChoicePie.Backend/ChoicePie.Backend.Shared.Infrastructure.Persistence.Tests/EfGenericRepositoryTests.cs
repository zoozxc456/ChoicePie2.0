using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests;

public sealed class EfGenericRepositoryTests
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
    public async Task AddAsync_GivenNewEntity_WhenSaved_ThenCanBeReadBackByGetByIdAsync()
    {
        await using var context = _fixture.CreateContext();
        var repository = new TestWidgetRepository(context);
        var widget = TestWidget.Create("Widget A");

        await repository.AddAsync(widget);
        await context.SaveChangesAsync();

        await using var readContext = _fixture.CreateContext();
        var loaded = await new TestWidgetRepository(readContext).GetByIdAsync(widget.Id);
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Name, Is.EqualTo("Widget A"));
    }

    [Test]
    public async Task UpdateAsync_GivenTrackedEntityMutatedInPlace_WhenSaved_ThenPersistsTheChange()
    {
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new TestWidgetRepository(writeContext);
        var widget = TestWidget.Create("Original Name");
        await writeRepository.AddAsync(widget);
        await writeContext.SaveChangesAsync();

        await using var updateContext = _fixture.CreateContext();
        var updateRepository = new TestWidgetRepository(updateContext);
        var loaded = await updateRepository.GetByIdAsync(widget.Id);
        loaded!.Rename("Renamed");
        await updateContext.SaveChangesAsync();

        await using var verifyContext = _fixture.CreateContext();
        var verified = await new TestWidgetRepository(verifyContext).GetByIdAsync(widget.Id);
        Assert.That(verified!.Name, Is.EqualTo("Renamed"));
    }

    [Test]
    public async Task DeleteAsync_GivenExistingEntity_WhenSaved_ThenGetByIdAsyncReturnsNull()
    {
        await using var writeContext = _fixture.CreateContext();
        var writeRepository = new TestWidgetRepository(writeContext);
        var widget = TestWidget.Create("To Delete");
        await writeRepository.AddAsync(widget);
        await writeContext.SaveChangesAsync();

        await using var deleteContext = _fixture.CreateContext();
        var deleteRepository = new TestWidgetRepository(deleteContext);
        var loaded = await deleteRepository.GetByIdAsync(widget.Id);
        await deleteRepository.DeleteAsync(loaded!);
        await deleteContext.SaveChangesAsync();

        await using var verifyContext = _fixture.CreateContext();
        var verified = await new TestWidgetRepository(verifyContext).GetByIdAsync(widget.Id);
        Assert.That(verified, Is.Null);
    }

    [Test]
    public async Task GetByIdAsync_GivenNonExistentId_WhenCalled_ThenReturnsNull()
    {
        await using var context = _fixture.CreateContext();
        var repository = new TestWidgetRepository(context);

        var result = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task ListAsync_GivenSpecificationMatchingSomeRows_WhenCalled_ThenReturnsOnlyMatchingRows()
    {
        await using var context = _fixture.CreateContext();
        var repository = new TestWidgetRepository(context);
        var uniquePrefix = $"Match-{Guid.NewGuid():N}";
        await repository.AddAsync(TestWidget.Create($"{uniquePrefix}-1"));
        await repository.AddAsync(TestWidget.Create($"{uniquePrefix}-2"));
        await repository.AddAsync(TestWidget.Create("NoMatch"));
        await context.SaveChangesAsync();

        var result = await repository.ListAsync(new TestWidgetNameStartsWithSpecification(uniquePrefix));

        Assert.That(result, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task FirstOrDefaultAsync_GivenSpecificationMatchingOneRow_WhenCalled_ThenReturnsThatRow()
    {
        await using var context = _fixture.CreateContext();
        var repository = new TestWidgetRepository(context);
        var uniqueName = $"Unique-{Guid.NewGuid():N}";
        await repository.AddAsync(TestWidget.Create(uniqueName));
        await context.SaveChangesAsync();

        var result = await repository.FirstOrDefaultAsync(new TestWidgetNameStartsWithSpecification(uniqueName));

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(uniqueName));
    }

    [Test]
    public async Task ExistsAsync_GivenSpecificationWithNoMatch_WhenCalled_ThenReturnsFalse()
    {
        await using var context = _fixture.CreateContext();
        var repository = new TestWidgetRepository(context);

        var result = await repository.ExistsAsync(new TestWidgetNameStartsWithSpecification($"Nonexistent-{Guid.NewGuid():N}"));

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task CountAsync_GivenSpecificationMatchingMultipleRows_WhenCalled_ThenReturnsCorrectCount()
    {
        await using var context = _fixture.CreateContext();
        var repository = new TestWidgetRepository(context);
        var uniquePrefix = $"Count-{Guid.NewGuid():N}";
        await repository.AddAsync(TestWidget.Create($"{uniquePrefix}-1"));
        await repository.AddAsync(TestWidget.Create($"{uniquePrefix}-2"));
        await repository.AddAsync(TestWidget.Create($"{uniquePrefix}-3"));
        await context.SaveChangesAsync();

        var result = await repository.CountAsync(new TestWidgetNameStartsWithSpecification(uniquePrefix));

        Assert.That(result, Is.EqualTo(3));
    }
}
