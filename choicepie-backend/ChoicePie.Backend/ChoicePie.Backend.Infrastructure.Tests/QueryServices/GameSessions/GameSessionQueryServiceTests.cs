using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Infrastructure.QueryServices.GameSessions;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using NSubstitute;
using GameRoomAggregate = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoom;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;

namespace ChoicePie.Backend.Infrastructure.Tests.QueryServices.GameSessions;

[TestFixture]
public class GameSessionQueryServiceTests
{
    private static readonly Guid HostUserId = Guid.NewGuid();
    private static readonly Guid QuizId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    private IReadRepository _readRepository = null!;
    private GameSessionQueryService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _readRepository = Substitute.For<IReadRepository>();
        _sut = new GameSessionQueryService(_readRepository);
    }

    private static GameSessionAggregate BuildPlayedSession(Guid? winnerMemberId = null, Guid? loserMemberId = null)
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = GameRoomAggregate.Create(
            HostUserId, "ABC123", QuizId, "測試題庫", "📝", "grad", questions, 20, CreatedAtUtc);
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var winner = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30), winnerMemberId);
        var loser = room.Join("小華", "connection-2", CreatedAtUtc.AddSeconds(35), loserMemberId);
        room.StartGame(HostUserId, startedAt);
        room.SubmitAnswer(winner.Id, answerIndex: 1, startedAt.AddSeconds(2));
        room.SubmitAnswer(loser.Id, answerIndex: 0, startedAt.AddSeconds(2));
        room.EndCurrentQuestion(HostUserId, startedAt.AddSeconds(20));
        room.AdvanceToNextQuestion(HostUserId, startedAt.AddSeconds(25));

        return GameSessionAggregate.RecordFrom(room, CreatedAtUtc.AddMinutes(2));
    }

    [Test]
    public async Task GetHostedByUserIdAsync_GivenSessionHostedByUser_WhenCalled_ThenReturnsItInSummary()
    {
        var session = BuildPlayedSession();
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate> { session }.AsQueryable());

        var result = await _sut.GetHostedByUserIdAsync(HostUserId, 1, 20, CancellationToken.None);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().RoomCode, Is.EqualTo("ABC123"));
        Assert.That(result.Items.First().TopPlayerName, Is.EqualTo("小明"));
        Assert.That(result.Items.First().MyRank, Is.Null, "hosted 列表不代入 host 自己的名次");
    }

    [Test]
    public async Task GetHostedByUserIdAsync_GivenSessionHostedByOtherUser_WhenCalled_ThenExcludesIt()
    {
        var session = BuildPlayedSession();
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate> { session }.AsQueryable());

        var result = await _sut.GetHostedByUserIdAsync(Guid.NewGuid(), 1, 20, CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetPlayedByMemberIdAsync_GivenLoggedInPlayer_WhenCalled_ThenReturnsSessionWithMyRankAndScore()
    {
        var memberId = Guid.NewGuid();
        var session = BuildPlayedSession(memberId);
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate> { session }.AsQueryable());

        var result = await _sut.GetPlayedByMemberIdAsync(memberId, 1, 20, CancellationToken.None);

        Assert.That(result.Items.Count(), Is.EqualTo(1));
        Assert.That(result.Items.First().MyRank, Is.EqualTo(1));
    }

    [Test]
    public async Task GetPlayedByMemberIdAsync_GivenAnonymousPlayerWithNoMemberId_WhenCalledByAnyMember_ThenNeverMatches()
    {
        var session = BuildPlayedSession(winnerMemberId: null);
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate> { session }.AsQueryable());

        var result = await _sut.GetPlayedByMemberIdAsync(Guid.NewGuid(), 1, 20, CancellationToken.None);

        Assert.That(result.Items, Is.Empty);
    }

    [Test]
    public async Task GetByIdAsync_GivenHost_WhenCalled_ThenReturnsIsHostTrueAndFullRankings()
    {
        var session = BuildPlayedSession();
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate> { session }.AsQueryable());

        var result = await _sut.GetByIdAsync(session.Id, HostUserId, CancellationToken.None);

        Assert.That(result!.IsHost, Is.True);
        Assert.That(result.Rankings, Has.Count.EqualTo(2));
        Assert.That(result.MyRank, Is.Null, "host 沒有玩，MyRank 應該是 null");
    }

    [Test]
    public async Task GetByIdAsync_GivenParticipatingPlayerWhoAnsweredWrong_WhenCalled_ThenIncludesWrongAnswer()
    {
        var loserMemberId = Guid.NewGuid();
        var session = BuildPlayedSession(loserMemberId: loserMemberId);
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate> { session }.AsQueryable());

        var result = await _sut.GetByIdAsync(session.Id, loserMemberId, CancellationToken.None);

        Assert.That(result!.IsHost, Is.False);
        Assert.That(result.MyRank, Is.EqualTo(2));
        Assert.That(result.MyWrongAnswers, Has.Count.EqualTo(1));
        Assert.That(result.MyWrongAnswers[0].MyAnswerIndex, Is.EqualTo(0));
        Assert.That(result.MyWrongAnswers[0].CorrectAnswerIndex, Is.EqualTo(1));
    }

    [Test]
    public async Task GetByIdAsync_GivenNonExistentId_WhenCalled_ThenReturnsNull()
    {
        _readRepository.Query<GameSessionAggregate>().Returns(new List<GameSessionAggregate>().AsQueryable());

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        Assert.That(result, Is.Null);
    }
}
