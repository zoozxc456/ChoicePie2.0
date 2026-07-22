using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetQuizForAttemptQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private GetQuizForAttemptQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _sut = new GetQuizForAttemptQueryHandler(_quizQueryService);
    }

    [Test]
    public async Task Handle_GivenServiceReturnsQuiz_WhenCalled_ThenReturnsIt()
    {
        var quizId = Guid.NewGuid();
        var dto = new QuizForAttemptDto(quizId, "Kubernetes 101", null, "⚓", "g", "beginner", Guid.NewGuid(), "Host Name", null, [], []);
        _quizQueryService.GetForAttemptAsync(quizId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetQuizForAttemptQuery(quizId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenServiceReturnsNull_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var quizId = Guid.NewGuid();
        _quizQueryService.GetForAttemptAsync(quizId, Arg.Any<CancellationToken>()).Returns((QuizForAttemptDto?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new GetQuizForAttemptQuery(quizId), CancellationToken.None));
    }
}
