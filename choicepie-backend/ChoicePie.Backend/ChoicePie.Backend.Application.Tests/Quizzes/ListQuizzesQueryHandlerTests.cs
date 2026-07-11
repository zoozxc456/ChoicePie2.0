using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Shared.Application.Contracts;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class ListQuizzesQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private ListQuizzesQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _sut = new ListQuizzesQueryHandler(_quizQueryService);
    }

    [Test]
    public async Task Handle_WhenCalled_ThenDelegatesToQuizQueryServiceWithRequestParametersAndReturnsItsResult()
    {
        var expected = new PagedResult<QuizSummaryDto>([], 2, 5, 0);
        _quizQueryService.ListAsync("Kubernetes", "search-term", 2, 5, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(
            new ListQuizzesQuery { Tag = "Kubernetes", Search = "search-term", PageNumber = 2, PageSize = 5 },
            CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }
}
