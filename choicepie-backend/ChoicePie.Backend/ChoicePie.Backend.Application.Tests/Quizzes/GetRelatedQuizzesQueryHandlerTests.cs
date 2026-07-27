using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Queries;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetRelatedQuizzesQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private GetRelatedQuizzesQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _sut = new GetRelatedQuizzesQueryHandler(_quizQueryService);
    }

    [Test]
    public async Task Handle_WhenCalled_ThenReturnsQuizQueryServiceResult()
    {
        var quizId = Guid.NewGuid();
        IReadOnlyList<QuizSummaryDto> expected =
        [
            new QuizSummaryDto(
                Guid.NewGuid(), "Related Quiz", null, "🎯", "from-red-500 to-orange-500", "beginner", "published",
                5, 10, 0.8m, Guid.NewGuid(), "Someone", null, ["tag"], DateTime.UtcNow, DateTime.UtcNow)
        ];
        _quizQueryService.GetRelatedAsync(quizId, 6, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetRelatedQuizzesQuery(quizId, 6), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }
}
