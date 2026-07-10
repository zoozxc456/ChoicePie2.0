using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetQuizTagsQueryHandlerTests
{
    private IReadRepository _readRepository = null!;
    private GetQuizTagsQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new GetQuizTagsQueryHandler(_readRepository);
    }

    [Test]
    public async Task Handle_WhenCalled_ThenReturnsDistinctSortedTagsFromPublicQuizzesOnly()
    {
        var creatorId = Guid.NewGuid();
        var quizzes = new List<Quiz>
        {
            Quiz.Create(creatorId, "Q1", null, "⚓", "g", Difficulty.Beginner, true, ["Go", "Kubernetes"]),
            Quiz.Create(creatorId, "Q2", null, "⚓", "g", Difficulty.Beginner, true, ["go", "AWS"]),
            Quiz.Create(creatorId, "Q3", null, "⚓", "g", Difficulty.Beginner, false, ["SecretTag"])
        };
        _readRepository.Query<Quiz>().Returns(quizzes.AsQueryable());

        var result = await _sut.Handle(new GetQuizTagsQuery(), CancellationToken.None);

        Assert.That(result, Is.EqualTo(new[] { "AWS", "Go", "Kubernetes" }));
    }
}
