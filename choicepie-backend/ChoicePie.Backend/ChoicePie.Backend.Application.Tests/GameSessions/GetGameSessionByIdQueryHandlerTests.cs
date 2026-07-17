using ChoicePie.Backend.Application.GameSessions.Contracts;
using ChoicePie.Backend.Application.GameSessions.Dtos;
using ChoicePie.Backend.Application.GameSessions.Queries;
using ChoicePie.Backend.Domain.Aggregates.GameSession.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.GameSessions;

[TestFixture]
public class GetGameSessionByIdQueryHandlerTests
{
    private IGameSessionQueryService _gameSessionQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private GetGameSessionByIdQueryHandler _sut = null!;
    private readonly Guid _userId = Guid.NewGuid();

    [SetUp]
    public void SetUp()
    {
        _gameSessionQueryService = Substitute.For<IGameSessionQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _sut = new GetGameSessionByIdQueryHandler(_gameSessionQueryService, _currentUserService);

        _currentUserService.UserId.Returns(_userId);
    }

    private static GameSessionDetailDto MakeDto(Guid sessionId, bool isHost, int? myRank) => new(
        sessionId, "ABCDEF", Guid.NewGuid(), "Quiz Title", "🎯", "grad",
        DateTime.UtcNow, 2, 1, isHost, [], myRank, myRank is null ? null : 100, []);

    [Test]
    public async Task Handle_GivenHost_WhenCalled_ThenReturnsSession()
    {
        var sessionId = Guid.NewGuid();
        var dto = MakeDto(sessionId, isHost: true, myRank: null);
        _gameSessionQueryService.GetByIdAsync(sessionId, _userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetGameSessionByIdQuery(sessionId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public async Task Handle_GivenParticipatingPlayer_WhenCalled_ThenReturnsSession()
    {
        var sessionId = Guid.NewGuid();
        var dto = MakeDto(sessionId, isHost: false, myRank: 1);
        _gameSessionQueryService.GetByIdAsync(sessionId, _userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await _sut.Handle(new GetGameSessionByIdQuery(sessionId), CancellationToken.None);

        Assert.That(result, Is.SameAs(dto));
    }

    [Test]
    public void Handle_GivenServiceReturnsNull_WhenCalled_ThenThrowsGameSessionNotFoundException()
    {
        var sessionId = Guid.NewGuid();
        _gameSessionQueryService.GetByIdAsync(sessionId, _userId, Arg.Any<CancellationToken>()).Returns((GameSessionDetailDto?)null);

        Assert.ThrowsAsync<GameSessionNotFoundException>(() =>
            _sut.Handle(new GetGameSessionByIdQuery(sessionId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnrelatedUser_WhenCalled_ThenThrowsGameSessionForbiddenException()
    {
        var sessionId = Guid.NewGuid();
        var dto = MakeDto(sessionId, isHost: false, myRank: null);
        _gameSessionQueryService.GetByIdAsync(sessionId, _userId, Arg.Any<CancellationToken>()).Returns(dto);

        Assert.ThrowsAsync<GameSessionForbiddenException>(() =>
            _sut.Handle(new GetGameSessionByIdQuery(sessionId), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenNoCurrentUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            _sut.Handle(new GetGameSessionByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
