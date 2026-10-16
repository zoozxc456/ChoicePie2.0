using ChoicePie.Backend.Application.CreatorFollows.Queries;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.CreatorFollows;

[TestFixture]
public class ListMyFollowingQuizzesQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private ListMyFollowingQuizzesQueryHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new ListMyFollowingQuizzesQueryHandler(_quizQueryService, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedUser_WhenCalled_ThenReturnsFeedForCurrentUser()
    {
        _currentUserService.UserId.Returns(_userId);
        var expected = new PagedResult<QuizSummaryDto>([], 1, 20, 0);
        _quizQueryService.ListFollowingFeedAsync(_userId, 1, 20, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListMyFollowingQuizzesQuery(1, 20), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new ListMyFollowingQuizzesQuery(1, 20), CancellationToken.None));
    }
}
