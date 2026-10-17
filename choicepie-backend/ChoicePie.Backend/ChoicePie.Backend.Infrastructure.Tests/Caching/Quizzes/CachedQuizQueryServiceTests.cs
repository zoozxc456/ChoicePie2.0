using System.Text.Json;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Infrastructure.Caching.Quizzes;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Caching.Services;
using NSubstitute;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.MicrosoftHybridCache;

namespace ChoicePie.Backend.Infrastructure.Tests.Caching.Quizzes;

[TestFixture]
public class CachedQuizQueryServiceTests
{
    private IQuizQueryService _inner = null!;
    private FusionCache _fusionCache = null!;
    private HybridCachingService _cache = null!;
    private CachedQuizQueryService _sut = null!;
    private QuizCacheInvalidator _invalidator = null!;
    private readonly Guid _quizId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _inner = Substitute.For<IQuizQueryService>();
        // 只用 L1 記憶體的 FusionCache，測真實的命中／失效行為，不需要 Redis。
        _fusionCache = new FusionCache(new FusionCacheOptions());
        _cache = new HybridCachingService(new FusionHybridCache(_fusionCache));
        _sut = new CachedQuizQueryService(_inner, _cache);
        _invalidator = new QuizCacheInvalidator(_cache, Substitute.For<Shared.Infrastructure.Persistence.Repositories.IReadRepository>());
    }

    [TearDown]
    public void TearDown() => _fusionCache.Dispose();

    private static QuizDto MakeQuizDto(Guid id, string title = "Kubernetes 101") => new(
        id, title, null, null, "⚓", "primary", "Beginner", "published", 3, 0.5m, Guid.NewGuid(), "Host", null,
        [new QuestionDto(Guid.NewGuid(), "Q?", ["A", "B", "C", "D"], 0, "because")],
        [new QuestionStubDto(Guid.NewGuid())], 1, ["Kubernetes"], 2, DateTime.UtcNow, DateTime.UtcNow);

    private static QuizSummaryDto MakeSummary(Guid id) => new(
        id, "Related", null, null, "⚓", "primary", "Beginner", "published", 1, 0, 0m, Guid.NewGuid(), "Host", null,
        ["Kubernetes"], DateTime.UtcNow, DateTime.UtcNow);

    [Test]
    public async Task GetByIdAsync_GivenRepeatedCalls_WhenCalled_ThenQueriesSourceOnce()
    {
        var dto = MakeQuizDto(_quizId);
        _inner.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(dto);

        var first = await _sut.GetByIdAsync(_quizId, CancellationToken.None);
        var second = await _sut.GetByIdAsync(_quizId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(dto));
            Assert.That(second, Is.EqualTo(dto));
        });
        await _inner.Received(1).GetByIdAsync(_quizId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetByIdAsync_GivenQuizInvalidated_WhenCalledAgain_ThenQueriesSourceAgain()
    {
        _inner.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(MakeQuizDto(_quizId, "Old"),
            MakeQuizDto(_quizId, "New"));
        await _sut.GetByIdAsync(_quizId, CancellationToken.None);

        await _invalidator.InvalidateQuizAsync(_quizId, CancellationToken.None);
        var result = await _sut.GetByIdAsync(_quizId, CancellationToken.None);

        Assert.That(result!.Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task GetByIdAsync_GivenOtherQuizInvalidated_WhenCalledAgain_ThenStillServedFromCache()
    {
        _inner.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns(MakeQuizDto(_quizId));
        await _sut.GetByIdAsync(_quizId, CancellationToken.None);

        await _invalidator.InvalidateQuizAsync(Guid.NewGuid(), CancellationToken.None);
        await _sut.GetByIdAsync(_quizId, CancellationToken.None);

        await _inner.Received(1).GetByIdAsync(_quizId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetByIdAsync_GivenMissingQuiz_WhenCalledTwice_ThenCachesTheMiss()
    {
        _inner.GetByIdAsync(_quizId, Arg.Any<CancellationToken>()).Returns((QuizDto?)null);

        var first = await _sut.GetByIdAsync(_quizId, CancellationToken.None);
        var second = await _sut.GetByIdAsync(_quizId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.Null);
            Assert.That(second, Is.Null);
        });
        await _inner.Received(1).GetByIdAsync(_quizId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetForAttemptAsync_GivenQuizInvalidated_WhenCalledAgain_ThenQueriesSourceAgain()
    {
        _inner.GetForAttemptAsync(_quizId, Arg.Any<CancellationToken>()).Returns((QuizForAttemptDto?)null);
        await _sut.GetForAttemptAsync(_quizId, CancellationToken.None);
        await _sut.GetForAttemptAsync(_quizId, CancellationToken.None);

        await _invalidator.InvalidateQuizAsync(_quizId, CancellationToken.None);
        await _sut.GetForAttemptAsync(_quizId, CancellationToken.None);

        await _inner.Received(2).GetForAttemptAsync(_quizId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetTagsAsync_GivenCatalogInvalidated_WhenCalledAgain_ThenQueriesSourceAgain()
    {
        _inner.GetTagsAsync(Arg.Any<CancellationToken>()).Returns(["Go"], ["Go", "Rust"]);
        await _sut.GetTagsAsync(CancellationToken.None);
        await _sut.GetTagsAsync(CancellationToken.None);

        await _invalidator.InvalidateCatalogAsync(CancellationToken.None);
        var result = await _sut.GetTagsAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { "Go", "Rust" }));
        await _inner.Received(2).GetTagsAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetRelatedAsync_GivenDifferentLimits_WhenCalled_ThenCachesEachSeparately()
    {
        _inner.GetRelatedAsync(_quizId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call => Enumerable.Range(0, call.ArgAt<int>(1)).Select(_ => MakeSummary(Guid.NewGuid())).ToList());

        var three = await _sut.GetRelatedAsync(_quizId, 3, CancellationToken.None);
        var five = await _sut.GetRelatedAsync(_quizId, 5, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(three, Has.Count.EqualTo(3));
            Assert.That(five, Has.Count.EqualTo(5));
        });
    }

    [TestCase(true, TestName = "GetRelatedAsync_GivenQuizInvalidated_WhenCalledAgain_ThenQueriesSourceAgain")]
    [TestCase(false, TestName = "GetRelatedAsync_GivenCatalogInvalidated_WhenCalledAgain_ThenQueriesSourceAgain")]
    public async Task GetRelatedAsync_GivenInvalidation_WhenCalledAgain_ThenQueriesSourceAgain(bool invalidateQuiz)
    {
        _inner.GetRelatedAsync(_quizId, 3, Arg.Any<CancellationToken>()).Returns([MakeSummary(Guid.NewGuid())]);
        await _sut.GetRelatedAsync(_quizId, 3, CancellationToken.None);
        await _sut.GetRelatedAsync(_quizId, 3, CancellationToken.None);

        if (invalidateQuiz)
        {
            await _invalidator.InvalidateQuizAsync(_quizId, CancellationToken.None);
        }
        else
        {
            await _invalidator.InvalidateCatalogAsync(CancellationToken.None);
        }

        await _sut.GetRelatedAsync(_quizId, 3, CancellationToken.None);

        await _inner.Received(2).GetRelatedAsync(_quizId, 3, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task ListAsync_GivenRepeatedCalls_WhenCalled_ThenAlwaysPassesThroughToSource()
    {
        _inner.ListAsync(null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns(new PagedResult<QuizSummaryDto>([], 1, 20, 0));

        await _sut.ListAsync(null, null, null, 1, 20, CancellationToken.None);
        await _sut.ListAsync(null, null, null, 1, 20, CancellationToken.None);

        await _inner.Received(2).ListAsync(null, null, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task AdminGetByIdAsync_GivenRepeatedCalls_WhenCalled_ThenAlwaysPassesThroughToSource()
    {
        await _sut.AdminGetByIdAsync(_quizId, CancellationToken.None);
        await _sut.AdminGetByIdAsync(_quizId, CancellationToken.None);

        await _inner.Received(2).AdminGetByIdAsync(_quizId, Arg.Any<CancellationToken>());
    }

    // L2 是 Redis，存進去的是 JSON；快取的 DTO 必須能來回序列化而不掉資料。
    [Test]
    public void CachedDtos_GivenSystemTextJsonRoundTrip_WhenDeserialized_ThenKeepAllValues()
    {
        var dto = MakeQuizDto(_quizId);

        var roundTripped = JsonSerializer.Deserialize<QuizDto>(JsonSerializer.Serialize(dto))!;

        Assert.Multiple(() =>
        {
            Assert.That(roundTripped with { Questions = dto.Questions, QuestionStubs = dto.QuestionStubs, Tags = dto.Tags },
                Is.EqualTo(dto));
            Assert.That(roundTripped.Questions[0] with { Options = dto.Questions[0].Options }, Is.EqualTo(dto.Questions[0]));
            Assert.That(roundTripped.Questions[0].Options, Is.EqualTo(dto.Questions[0].Options));
            Assert.That(roundTripped.QuestionStubs, Is.EqualTo(dto.QuestionStubs));
            Assert.That(roundTripped.Tags, Is.EqualTo(dto.Tags));
        });
    }
}
