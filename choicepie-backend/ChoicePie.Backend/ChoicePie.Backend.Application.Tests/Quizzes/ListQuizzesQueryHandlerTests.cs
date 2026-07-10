using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class ListQuizzesQueryHandlerTests
{
    private IReadRepository _readRepository = null!;
    private ListQuizzesQueryHandler _sut = null!;
    private Member _creator = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new ListQuizzesQueryHandler(_readRepository);
        _creator = Member.Register(Email.Create("host@example.com"), "Host Name", "hashed");
        _readRepository.Query<Member>().Returns(new List<Member> { _creator }.AsQueryable());
    }

    private Quiz MakeQuiz(string title, bool isPublic = true, params string[] tags) =>
        Quiz.Create(_creator.Id, title, null, "⚓", "g", Difficulty.Beginner, isPublic, tags);

    [Test]
    public async Task Handle_GivenNoFilters_WhenCalled_ThenReturnsOnlyPublicQuizzes()
    {
        var publicQuiz = MakeQuiz("Public Quiz");
        var privateQuiz = MakeQuiz("Private Quiz", isPublic: false);
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { publicQuiz, privateQuiz }.AsQueryable());

        var result = await _sut.Handle(new ListQuizzesQuery(), CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Public Quiz" }));
    }

    [Test]
    public async Task Handle_GivenTagFilter_WhenCalled_ThenReturnsOnlyMatchingQuizzes()
    {
        var kubeQuiz = MakeQuiz("Kube Quiz", tags: "Kubernetes");
        var goQuiz = MakeQuiz("Go Quiz", tags: "Go");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { kubeQuiz, goQuiz }.AsQueryable());

        var result = await _sut.Handle(new ListQuizzesQuery { Tag = "Kubernetes" }, CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Kube Quiz" }));
    }

    [Test]
    public async Task Handle_GivenSearchFilter_WhenCalled_ThenReturnsOnlyMatchingTitles()
    {
        var quiz1 = MakeQuiz("Kubernetes Basics");
        var quiz2 = MakeQuiz("Go Fundamentals");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { quiz1, quiz2 }.AsQueryable());

        var result = await _sut.Handle(new ListQuizzesQuery { Search = "Kubernetes" }, CancellationToken.None);

        Assert.That(result.Items.Select(i => i.Title), Is.EquivalentTo(new[] { "Kubernetes Basics" }));
    }

    [Test]
    public async Task Handle_GivenMoreItemsThanPageSize_WhenCalled_ThenReturnsCorrectPageAndTotalCount()
    {
        var quizzes = Enumerable.Range(1, 5).Select(i => MakeQuiz($"Quiz {i}")).ToList();
        _readRepository.Query<Quiz>().Returns(quizzes.AsQueryable());

        var result = await _sut.Handle(new ListQuizzesQuery { PageNumber = 1, PageSize = 2 }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Count(), Is.EqualTo(2));
            Assert.That(result.TotalCount, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task Handle_WhenCalled_ThenPopulatesCreatorInfo()
    {
        var quiz = MakeQuiz("Solo Quiz");
        _readRepository.Query<Quiz>().Returns(new List<Quiz> { quiz }.AsQueryable());

        var result = await _sut.Handle(new ListQuizzesQuery(), CancellationToken.None);

        Assert.That(result.Items.Single().CreatorName, Is.EqualTo("Host Name"));
    }
}
