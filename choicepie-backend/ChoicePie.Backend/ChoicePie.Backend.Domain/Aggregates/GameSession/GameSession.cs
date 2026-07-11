using ChoicePie.Backend.Domain.Aggregates.GameSession.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.GameSession;

public sealed class GameSession : AggregateRoot<Guid>
{
    private readonly List<GameSessionQuestion> _questions = [];
    private readonly List<GameSessionPlayerResult> _playerResults = [];

    public string RoomCode { get; private set; } = null!;
    public Guid HostUserId { get; private set; }
    public DateTime PlayedAtUtc { get; private set; }

    public IReadOnlyList<GameSessionQuestion> Questions => _questions.AsReadOnly();
    public IReadOnlyList<GameSessionPlayerResult> PlayerResults => _playerResults.AsReadOnly();

    private GameSession()
    {
    }

    public static GameSession RecordFrom(GameRoom.GameRoom room, DateTime utcNow)
    {
        var session = new GameSession
        {
            Id = Guid.NewGuid(),
            RoomCode = room.RoomCode,
            HostUserId = room.HostUserId,
            PlayedAtUtc = utcNow
        };

        session._questions.AddRange(room.Questions.Select(q =>
            new GameSessionQuestion(q.QuestionId, q.Text, q.Options, q.AnswerIndex, q.Explanation)));

        var rankedPlayers = room.Players.OrderByDescending(p => p.Score).ToList();
        for (var i = 0; i < rankedPlayers.Count; i++)
        {
            var player = rankedPlayers[i];
            var answers = player.Answers
                .Select(kv => new GameSessionAnswerLogEntry(
                    kv.Key, kv.Value.AnswerIndex, kv.Value.IsCorrect, kv.Value.Score))
                .OrderBy(a => a.QuestionIndex)
                .ToList();

            session._playerResults.Add(new GameSessionPlayerResult(
                player.Id, player.Nickname, player.Score, Rank: i + 1, answers));
        }

        return session;
    }
}
