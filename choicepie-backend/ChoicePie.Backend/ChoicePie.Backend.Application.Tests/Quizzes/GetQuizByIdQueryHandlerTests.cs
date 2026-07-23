using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Application.Quizzes.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GetQuizByIdQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetQuizByIdQueryHandler _sut = null!;
    private readonly Guid _ownerId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetQuizByIdQueryHandler(_quizQueryService, _currentUserService);

        _currentUserService.UserId.Returns(_ownerId);
    }

    private QuizDto MakeDto(Guid quizId) => new(
        quizId, "Kubernetes 101", null, "⚓", "g", "beginner", "draft", 0, 0, _ownerId, "Host Name", null, [], [], 0,
        DateTime.UtcNow, DateTime.UtcNow);

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenReturnsQuiz()
    {
        var quizId = Guid.NewGuid();
        var dto = MakeDto(quizId);
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenServiceReturnsNull_WhenCalled_ThenThrowsQuizNotFoundException()
    {
        var quizId = Guid.NewGuid();
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns((QuizDto?)null);

        Assert.ThrowsAsync<QuizNotFoundException>(() => _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizForbiddenException()
    {
        var quizId = Guid.NewGuid();
        var dto = MakeDto(quizId);
        _quizQueryService.GetByIdAsync(quizId, Arg.Any<CancellationToken>()).Returns(dto);
        _currentUserService.UserId.Returns(Guid.NewGuid());

        Assert.ThrowsAsync<QuizForbiddenException>(() => _sut.Handle(new GetQuizByIdQuery(quizId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() => _sut.Handle(new GetQuizByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
