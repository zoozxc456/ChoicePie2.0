using ChoicePie.Backend.Application.QuizAttempts.Contracts;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Application.QuizAttempts.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.QuizAttempts;

[TestFixture]
public class ListQuizAttemptHistoryQueryHandlerTests
{
    private IQuizAttemptQueryService _quizAttemptQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private ListQuizAttemptHistoryQueryHandler _sut = null!;
    private readonly Guid _memberId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizAttemptQueryService = Substitute.For<IQuizAttemptQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new ListQuizAttemptHistoryQueryHandler(_quizAttemptQueryService, _currentUserService);

        _currentUserService.UserId.Returns(_memberId);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedMember_WhenCalled_ThenReturnsHistoryForThatMember()
    {
        var quizId = Guid.NewGuid();
        var items = new List<QuizAttemptHistoryItemDto>
        {
            new(Guid.NewGuid(), 80m, true, DateTime.UtcNow, DateTime.UtcNow, 12000)
        };
        _quizAttemptQueryService.ListHistoryAsync(quizId, _memberId, Arg.Any<CancellationToken>()).Returns(items);

        var result = await _sut.Handle(new ListQuizAttemptHistoryQuery(quizId), CancellationToken.None);

        Assert.That(result, Is.SameAs(items));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new ListQuizAttemptHistoryQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
