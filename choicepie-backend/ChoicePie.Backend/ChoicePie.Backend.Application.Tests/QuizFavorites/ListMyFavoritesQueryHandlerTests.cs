using ChoicePie.Backend.Application.QuizFavorites.Queries;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.QuizFavorites;

[TestFixture]
public class ListMyFavoritesQueryHandlerTests
{
    private IQuizQueryService _quizQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private ListMyFavoritesQueryHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _quizQueryService = Substitute.For<IQuizQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new ListMyFavoritesQueryHandler(_quizQueryService, _currentUserService);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedUser_WhenCalled_ThenReturnsFavoritesForCurrentUser()
    {
        _currentUserService.UserId.Returns(_userId);
        var expected = new PagedResult<QuizSummaryDto>([], 2, 10, 0);
        _quizQueryService.ListFavoritesAsync(_userId, 2, 10, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new ListMyFavoritesQuery(2, 10), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new ListMyFavoritesQuery(1, 20), CancellationToken.None));
    }
}
