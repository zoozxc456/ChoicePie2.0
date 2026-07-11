using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Queries;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetQuizTagsQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private GetQuizTagsQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _sut = new GetQuizTagsQueryHandler(_quizQueryService);
    }

    [Test]
    public async Task Handle_WhenCalled_ThenReturnsQuizQueryServiceResult()
    {
        IReadOnlyList<string> expected = ["AWS", "Go", "Kubernetes"];
        _quizQueryService.GetTagsAsync(Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetQuizTagsQuery(), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }
}
