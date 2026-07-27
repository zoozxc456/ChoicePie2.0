using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Application.GameSessions.Queries;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameSessions;

[TestFixture]
public class GetPlayedGameSessionsQueryHandlerTests
{
    private IGameSessionQueryService _gameSessionQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetPlayedGameSessionsQueryHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _gameSessionQueryService = Substitute.For<IGameSessionQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetPlayedGameSessionsQueryHandler(_gameSessionQueryService, _currentUserService);

        _currentUserService.UserId.Returns(_userId);
    }

    [Test]
    public async Task Handle_GivenAuthenticatedUser_WhenCalled_ThenReturnsPlayedSessionsForCurrentUser()
    {
        var expected = new PagedResult<GameSessionSummaryDto>([], 1, 20, 0);
        _gameSessionQueryService.GetPlayedByMemberIdAsync(_userId, 1, 20, Arg.Any<CancellationToken>()).Returns(expected);

        var result = await _sut.Handle(new GetPlayedGameSessionsQuery(1, 20), CancellationToken.None);

        Assert.That(result, Is.SameAs(expected));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new GetPlayedGameSessionsQuery(1, 20), CancellationToken.None));
    }
}
