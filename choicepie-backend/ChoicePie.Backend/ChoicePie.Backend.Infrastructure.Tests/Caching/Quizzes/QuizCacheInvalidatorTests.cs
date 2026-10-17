using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.Caching.Quizzes;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.Caching.Quizzes;

[TestFixture]
public class QuizCacheInvalidatorTests
{
    private ICachingService _cache = null!;
    private IReadRepository _readRepository = null!;
    private QuizCacheInvalidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = Substitute.For<ICachingService>();
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new QuizCacheInvalidator(_cache, _readRepository);
    }

    private static Quiz MakeQuiz(Guid creatorId) =>
        Quiz.Create(creatorId, "Kubernetes 101", null, null, "⚓", "primary", Difficulty.Beginner, ["Kubernetes"]);

    [Test]
    public async Task InvalidateCreatorAsync_GivenCreatorWithQuizzes_WhenCalled_ThenInvalidatesOnlyThatCreatorsQuizzes()
    {
        var creatorId = Guid.NewGuid();
        var own1 = MakeQuiz(creatorId);
        var own2 = MakeQuiz(creatorId);
        var others = MakeQuiz(Guid.NewGuid());
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { own1, others, own2 }.AsQueryable());

        await _sut.InvalidateCreatorAsync(creatorId, CancellationToken.None);

        await _cache.Received(1).RemoveByTagAsync(QuizCacheKeys.QuizTag(own1.Id), Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveByTagAsync(QuizCacheKeys.QuizTag(own2.Id), Arg.Any<CancellationToken>());
        await _cache.DidNotReceive().RemoveByTagAsync(QuizCacheKeys.QuizTag(others.Id), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task InvalidateCreatorAsync_GivenCreatorWithoutQuizzes_WhenCalled_ThenRemovesNothing()
    {
        _readRepository.Query<Quiz>().Returns(new List<Quiz>().AsQueryable());

        await _sut.InvalidateCreatorAsync(Guid.NewGuid(), CancellationToken.None);

        await _cache.DidNotReceiveWithAnyArgs().RemoveByTagAsync(default!, default);
    }
}
