using ChoicePie.Backend.Application.QuizAttempts.Contracts;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Application.QuizAttempts.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.QuizAttempts;

[TestFixture]
public class GetQuizAttemptByIdQueryHandlerTests
{
    private IQuizAttemptQueryService _quizAttemptQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetQuizAttemptByIdQueryHandler _sut = null!;
    private readonly Guid _memberId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizAttemptQueryService = Substitute.For<IQuizAttemptQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetQuizAttemptByIdQueryHandler(_quizAttemptQueryService, _currentUserService);

        _currentUserService.UserId.Returns(_memberId);
    }

    [Test]
    public async Task Handle_GivenOwner_WhenCalled_ThenReturnsResult()
    {
        var attemptId = Guid.NewGuid();
        var dto = new QuizAttemptResultDto(attemptId, Guid.NewGuid(), "Title", _memberId, 100m, true, DateTime.UtcNow, DateTime.UtcNow, []);
        _quizAttemptQueryService.GetByIdAsync(attemptId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetQuizAttemptByIdQuery(attemptId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenServiceReturnsNull_WhenCalled_ThenThrowsQuizAttemptNotFoundException()
    {
        var attemptId = Guid.NewGuid();
        _quizAttemptQueryService.GetByIdAsync(attemptId, Arg.Any<CancellationToken>()).Returns((QuizAttemptResultDto?)null);

        Assert.ThrowsAsync<QuizAttemptNotFoundException>(() =>
            _sut.Handle(new GetQuizAttemptByIdQuery(attemptId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNonOwner_WhenCalled_ThenThrowsQuizAttemptForbiddenException()
    {
        var attemptId = Guid.NewGuid();
        var dto = new QuizAttemptResultDto(attemptId, Guid.NewGuid(), "Title", Guid.NewGuid(), 100m, true, DateTime.UtcNow, DateTime.UtcNow, []);
        _quizAttemptQueryService.GetByIdAsync(attemptId, Arg.Any<CancellationToken>()).Returns(dto);

        Assert.ThrowsAsync<QuizAttemptForbiddenException>(() =>
            _sut.Handle(new GetQuizAttemptByIdQuery(attemptId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new GetQuizAttemptByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
