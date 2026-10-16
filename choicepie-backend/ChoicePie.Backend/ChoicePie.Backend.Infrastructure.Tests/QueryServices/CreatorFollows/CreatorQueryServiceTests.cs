using ChoicePie.Backend.Domain.Aggregates.CreatorFollow;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.QueryServices.CreatorFollows;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.CreatorFollows;

[TestFixture]
public class CreatorQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private CreatorQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new CreatorQueryService(_readRepository);
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
    public async Task GetByIdAsync_GivenAnonymousCaller_WhenCalled_ThenIsFollowingIsFalse()
    {
        var creator = Member.Create("Alice");
        _readRepository.Query<Member>().Returns(new List<Member> { creator }.AsQueryable());
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { MakeQuiz(creator, "A") }.AsQueryable());
        _readRepository.Query<CreatorFollow>().Returns(new List<CreatorFollow>().AsQueryable());

        var result = await _sut.GetByIdAsync(creator.Id, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.IsFollowing, Is.False);
            Assert.That(result.QuizCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ListFollowingAsync_GivenFollows_WhenCalled_ThenReturnsFollowedCreatorsNewestFollowFirstWithPublishedQuizCount()
    {
        var userId = Guid.NewGuid();
        var alice = Member.Create("Alice");
        var bob = Member.Create("Bob");
        var carol = Member.Create("Carol");
        var follows = new List<CreatorFollow> { CreatorFollow.Create(userId, alice.Id) };
        await Task.Delay(5);
        follows.Add(CreatorFollow.Create(userId, bob.Id));
        follows.Add(CreatorFollow.Create(Guid.NewGuid(), carol.Id));
        _readRepository.Query<Member>().Returns(new List<Member> { alice, bob, carol }.AsQueryable());
        _readRepository.Query<CreatorFollow>().Returns(follows.AsQueryable());
        _readRepository.Query<Quiz>().Returns(new List<Quiz>
        {
            MakeQuiz(alice, "A1"), MakeQuiz(alice, "A2"), MakeQuiz(alice, "Draft", published: false)
        }.AsQueryable());

        var result = await _sut.ListFollowingAsync(userId, 1, 10, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Select(i => i.Name), Is.EqualTo(new[] { "Bob", "Alice" }));
            Assert.That(result.Items.Last().QuizCount, Is.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(2));
        });
    }
}
