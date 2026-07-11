using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameSession;

[TestFixture]
public class GameSessionTests
{
    private static readonly Guid HostUserId = Guid.NewGuid();
    private static readonly DateTime CreatedAtUtc = new(2026, 7, 11, 12, 0, 0, DateTimeKind.Utc);

    private static Domain.Aggregates.GameRoom.GameRoom BuildPlayedRoom()
    {
        var questions = new List<GameQuestionSnapshot>
        {
            new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], AnswerIndex: 1, "基本加法")
        };
        var room = Domain.Aggregates.GameRoom.GameRoom.Create(HostUserId, "ABC123", questions, 20, CreatedAtUtc);
        var startedAt = CreatedAtUtc.AddMinutes(1);
        var winner = room.Join("小明", "connection-1", CreatedAtUtc.AddSeconds(30));
        var loser = room.Join("小華", "connection-2", CreatedAtUtc.AddSeconds(35));
        room.StartGame(startedAt);
        room.SubmitAnswer(winner.Id, answerIndex: 1, startedAt.AddSeconds(2));
        room.SubmitAnswer(loser.Id, answerIndex: 0, startedAt.AddSeconds(2));
        room.EndCurrentQuestion(startedAt.AddSeconds(20));
        room.AdvanceToNextQuestion(startedAt.AddSeconds(25));

        return room;
    }

    [Test]
    public void RecordFrom_GivenEndedGameRoom_WhenCalled_ThenMapsRoomCodeHostAndQuestions()
    {
        var room = BuildPlayedRoom();
        var recordedAt = CreatedAtUtc.AddMinutes(2);

        var session = Domain.Aggregates.GameSession.GameSession.RecordFrom(room, recordedAt);

        Assert.Multiple(() =>
        {
            Assert.That(session.RoomCode, Is.EqualTo("ABC123"));
            Assert.That(session.HostUserId, Is.EqualTo(HostUserId));
            Assert.That(session.PlayedAtUtc, Is.EqualTo(recordedAt));
            Assert.That(session.Questions, Has.Count.EqualTo(1));
            Assert.That(session.Questions[0].Text, Is.EqualTo("1+1=?"));
        });
    }

    [Test]
    public void RecordFrom_GivenTwoPlayersWithDifferentScores_WhenCalled_ThenRanksPlayersByScoreDescending()
    {
        var room = BuildPlayedRoom();

        var session = Domain.Aggregates.GameSession.GameSession.RecordFrom(room, CreatedAtUtc.AddMinutes(2));

        var winner = session.PlayerResults.Single(p => p.Nickname == "小明");
        var loser = session.PlayerResults.Single(p => p.Nickname == "小華");

        Assert.Multiple(() =>
        {
            Assert.That(winner.FinalScore, Is.EqualTo(1000));
            Assert.That(winner.Rank, Is.EqualTo(1));
            Assert.That(loser.FinalScore, Is.EqualTo(0));
            Assert.That(loser.Rank, Is.EqualTo(2));
        });
    }

    [Test]
    public void RecordFrom_GivenPlayerWithWrongAnswer_WhenCalled_ThenAnswerLogRecordsIncorrectSelection()
    {
        var room = BuildPlayedRoom();

        var session = Domain.Aggregates.GameSession.GameSession.RecordFrom(room, CreatedAtUtc.AddMinutes(2));

        var loser = session.PlayerResults.Single(p => p.Nickname == "小華");
        var answer = loser.Answers.Single(a => a.QuestionIndex == 0);

        Assert.Multiple(() =>
        {
            Assert.That(answer.IsCorrect, Is.False);
            Assert.That(answer.SelectedOptionIndex, Is.EqualTo(0));
            Assert.That(answer.ScoreAwarded, Is.EqualTo(0));
        });
    }
}
