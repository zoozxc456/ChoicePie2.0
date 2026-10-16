using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Infrastructure.QueryServices.Quizzes;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.Quizzes;

// 收藏 / 追蹤動態 / 創作者作品清單
[TestFixture]
public class QuizQueryServiceMemberListsTests
{
    private IReadRepository _readRepository = null!;
    private QuizQueryService _sut = null!;
    private Member _creator = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new QuizQueryService(_readRepository, TimeProvider.System);
        _creator = Member.Create("Host Name");
        _readRepository.Query<Member>().Returns(new List<Member> { _creator }.AsQueryable());
    }

    private static Quiz MakeQuiz(Member creator, string title, bool published = true)
    {
        var quiz = Quiz.Create(creator.Id, title, null, null, "x", "primary", Difficulty.Beginner, []);
        if (published)
        {
            quiz.AddQuestion(Question.Create("Q?", ["A", "B", "C", "D"], 0, "because"));
            quiz.Publish();
        }

        return quiz;
    }

    [Test]
    public async Task ListFavoritesAsync_GivenFavorites_WhenCalled_ThenReturnsPublishedOnesNewestFavoriteFirst()
    {
        var userId = Guid.NewGuid();
        var first = MakeQuiz(_creator, "First");
        var second = MakeQuiz(_creator, "Second");
        var draft = MakeQuiz(_creator, "Draft", published: false);
        var notFavorited = MakeQuiz(_creator, "Other");
        var favorites = new List<QuizFavorite>();
        foreach (var quiz in new[] { first, draft, second })
        {
            favorites.Add(QuizFavorite.Create(quiz.Id, userId));
            await Task.Delay(5);
        }

        favorites.Add(QuizFavorite.Create(notFavorited.Id, Guid.NewGuid()));
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { first, second, draft, notFavorited }.AsQueryable());
        _readRepository.Query<QuizFavorite>().Returns(favorites.AsQueryable());

        var result = await _sut.ListFavoritesAsync(userId, 1, 10, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(i => i.Title), Is.EqualTo(new[] { "Second", "First" }));
            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(result.Items.First().CreatorName, Is.EqualTo("Host Name"));
        });
    }

    [Test]
    public async Task ListFavoritesAsync_GivenSecondPage_WhenCalled_ThenPagesResults()
    {
        var userId = Guid.NewGuid();
        var quizzes = new List<Quiz>();
        var favorites = new List<QuizFavorite>();
        for (var i = 0; i < 3; i++)
        {
            var quiz = MakeQuiz(_creator, $"Q{i}");
            quizzes.Add(quiz);
            favorites.Add(QuizFavorite.Create(quiz.Id, userId));
            await Task.Delay(5);
        }

        _readRepository.Query<Quiz>().Returns(quizzes.AsQueryable());
        _readRepository.Query<QuizFavorite>().Returns(favorites.AsQueryable());

        var result = await _sut.ListFavoritesAsync(userId, 2, 2, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(i => i.Title), Is.EqualTo(new[] { "Q0" }));
            Assert.That(result.TotalCount, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task ListFollowingFeedAsync_GivenFollows_WhenCalled_ThenReturnsPublishedQuizzesOfFollowedCreatorsNewestFirst()
    {
        var userId = Guid.NewGuid();
        var other = Member.Create("Other Creator");
        var stranger = Member.Create("Stranger");
        var older = MakeQuiz(_creator, "Older");
        await Task.Delay(5);
        var newer = MakeQuiz(other, "Newer");
        var draft = MakeQuiz(_creator, "Draft", published: false);
        var strangerQuiz = MakeQuiz(stranger, "Stranger quiz");
        _readRepository.Query<Member>().Returns(new List<Member> { _creator, other, stranger }.AsQueryable());
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { older, newer, draft, strangerQuiz }.AsQueryable());
        _readRepository.Query<CreatorFollow>().Returns(new List<CreatorFollow>
        {
            CreatorFollow.Create(userId, _creator.Id), CreatorFollow.Create(userId, other.Id)
        }.AsQueryable());

        var result = await _sut.ListFollowingFeedAsync(userId, 1, 10, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(i => i.Title), Is.EqualTo(new[] { "Newer", "Older" }));
            Assert.That(result.TotalCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task ListByCreatorAsync_GivenCreator_WhenCalled_ThenReturnsOnlyTheirPublishedQuizzesNewestFirst()
    {
        var other = Member.Create("Other Creator");
        var older = MakeQuiz(_creator, "Older");
        await Task.Delay(5);
        var newer = MakeQuiz(_creator, "Newer");
        var draft = MakeQuiz(_creator, "Draft", published: false);
        var otherQuiz = MakeQuiz(other, "Not mine");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { older, newer, draft, otherQuiz }.AsQueryable());

        var result = await _sut.ListByCreatorAsync(_creator.Id, 1, 10, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(i => i.Title), Is.EqualTo(new[] { "Newer", "Older" }));
            Assert.That(result.TotalCount, Is.EqualTo(2));
        });
    }
}
