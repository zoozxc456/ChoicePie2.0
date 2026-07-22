using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Infrastructure.QueryServices.Quizzes;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.Quizzes;

[TestFixture]
public class QuizQueryServiceTests
{
    private IReadRepository _readRepository = null!;
    private QuizQueryService _sut = null!;
    private Member _creator = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new QuizQueryService(_readRepository);
        _creator = Member.Create("Host Name");
        _readRepository.Query<Member>().Returns(new List<Member> { _creator }.AsQueryable());
    }

    private Quiz MakeQuiz(string title, bool published = true, params string[] tags)
    {
        var quiz = Quiz.Create(_creator.Id, title, null, "⚓", "g", Difficulty.Beginner, tags);
        if (published)
        {
            quiz.AddQuestion(Question.Create("Q?", ["A", "B", "C", "D"], 0, "because"));
            quiz.Publish();
        }

        return quiz;
    }

    [Test]
    public async Task GetByIdAsync_GivenExistingQuiz_WhenCalled_ThenReturnsQuizDtoWithCreatorInfo()
    {
        var quiz = MakeQuiz("Kubernetes 101");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { quiz }.AsQueryable());

        var result = await _sut.GetByIdAsync(quiz.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo("Kubernetes 101"));
            Assert.That(result.CreatorName, Is.EqualTo("Host Name"));
            Assert.That(result.Status, Is.EqualTo("published"));
        });
    }

    [Test]
    public async Task GetByIdAsync_GivenUnknownId_WhenCalled_ThenReturnsNull()
    {
        _readRepository.Query<Quiz>().Returns(new List<Quiz>().AsQueryable());

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetForAttemptAsync_GivenExistingQuiz_WhenCalled_ThenReturnsAnswerHiddenDto()
    {
        var quiz = MakeQuiz("Kubernetes 101");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { quiz }.AsQueryable());

        var result = await _sut.GetForAttemptAsync(quiz.Id, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result!.Title, Is.EqualTo("Kubernetes 101"));
            Assert.That(result.Questions.Single().Options, Is.EqualTo(new[] { "A", "B", "C", "D" }));
        });
    }

    [Test]
    public async Task ListAsync_GivenNoFilters_WhenCalled_ThenReturnsOnlyPublishedQuizzes()
    {
        var publicQuiz = MakeQuiz("Public Quiz");
        var draftQuiz = MakeQuiz("Draft Quiz", published: false);
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { publicQuiz, draftQuiz }.AsQueryable());

        var result = await _sut.ListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Public Quiz" }));
    }

    [Test]
    public async Task ListAsync_GivenOwnerId_WhenCalled_ThenReturnsOwnerQuizzesRegardlessOfStatus()
    {
        var published = MakeQuiz("Published Quiz");
        var draft = MakeQuiz("Draft Quiz", published: false);
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { published, draft }.AsQueryable());

        var result = await _sut.ListAsync(null, null, _creator.Id, 1, 10, CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Published Quiz", "Draft Quiz" }));
    }

    [Test]
    public async Task ListAsync_GivenTagFilter_WhenCalled_ThenReturnsOnlyMatchingQuizzes()
    {
        var kubeQuiz = MakeQuiz("Kube Quiz", tags: "Kubernetes");
        var goQuiz = MakeQuiz("Go Quiz", tags: "Go");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { kubeQuiz, goQuiz }.AsQueryable());

        var result = await _sut.ListAsync("Kubernetes", null, null, 1, 10, CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Kube Quiz" }));
    }

    [Test]
    public async Task ListAsync_GivenSearchFilter_WhenCalled_ThenReturnsOnlyMatchingTitles()
    {
        var quiz1 = MakeQuiz("Kubernetes Basics");
        var quiz2 = MakeQuiz("Go Fundamentals");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { quiz1, quiz2 }.AsQueryable());

        var result = await _sut.ListAsync(null, "Kubernetes", null, 1, 10, CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Kubernetes Basics" }));
    }

    [Test]
    public async Task ListAsync_GivenMoreItemsThanPageSize_WhenCalled_ThenReturnsCorrectPageAndTotalCount()
    {
        var quizzes = Enumerable.Range(1, 5).Select(i => MakeQuiz($"Quiz {i}")).ToList();
        _readRepository.Query<Quiz>().Returns(quizzes.AsQueryable());

        var result = await _sut.ListAsync(null, null, null, 1, 2, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Count(), Is.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task ListAsync_WhenCalled_ThenPopulatesCreatorInfo()
    {
        var quiz = MakeQuiz("Solo Quiz");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { quiz }.AsQueryable());

        var result = await _sut.ListAsync(null, null, null, 1, 10, CancellationToken.None);

        Assert.That(result.Items.Single().CreatorName, Is.EqualTo("Host Name"));
    }

    [Test]
    public async Task GetTagsAsync_WhenCalled_ThenReturnsDistinctSortedTagsFromPublishedQuizzesOnly()
    {
        var q1 = Quiz.Create(_creator.Id, "Q1", null, "⚓", "g", Difficulty.Beginner, ["Go", "Kubernetes"]);
        q1.AddQuestion(Question.Create("Q?", ["A", "B", "C", "D"], 0, "because"));
        q1.Publish();
        var q2 = Quiz.Create(_creator.Id, "Q2", null, "⚓", "g", Difficulty.Beginner, ["go", "AWS"]);
        q2.AddQuestion(Question.Create("Q?", ["A", "B", "C", "D"], 0, "because"));
        q2.Publish();
        var q3 = Quiz.Create(_creator.Id, "Q3", null, "⚓", "g", Difficulty.Beginner, ["SecretTag"]);
        var quizzes = new List<Quiz> { q1, q2, q3 };
        _readRepository.Query<Quiz>().Returns(quizzes.AsQueryable());

        var result = await _sut.GetTagsAsync(CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { "AWS", "Go", "Kubernetes" }));
    }
}
