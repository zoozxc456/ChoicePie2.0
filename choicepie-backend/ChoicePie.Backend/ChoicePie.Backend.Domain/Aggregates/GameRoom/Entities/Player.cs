using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Entities;

public sealed class Player : AuditableEntity<Guid>
{
    private readonly Dictionary<int, PlayerAnswer> _answersByQuestionIndex = [];

    public string Nickname { get; private set; } = null!;
    public string ConnectionId { get; private set; } = null!;
    public int Score { get; private set; }
    public int? SelectedOptionIndex { get; private set; }

    private Player()
    {
    }

    public static Player Create(string nickname, string connectionId) => new()
    {
        Id = Guid.NewGuid(),
        Nickname = nickname,
        ConnectionId = connectionId
    };

    public IReadOnlyDictionary<int, PlayerAnswer> Answers => _answersByQuestionIndex;

    public bool HasAnsweredQuestion(int questionIndex) => _answersByQuestionIndex.ContainsKey(questionIndex);

    public void RecordAnswer(int questionIndex, int answerIndex, int score, bool isCorrect)
    {
        _answersByQuestionIndex[questionIndex] = new PlayerAnswer(answerIndex, score, isCorrect);
        SelectedOptionIndex = answerIndex;
        Score += score;
    }

    public void ResetForNextQuestion() => SelectedOptionIndex = null;

    public PlayerMemento ToMemento() =>
        new(Id, Nickname, ConnectionId, Score, SelectedOptionIndex, _answersByQuestionIndex);

    public static Player Restore(PlayerMemento memento)
    {
        var player = new Player
        {
            Id = memento.Id,
            Nickname = memento.Nickname,
            ConnectionId = memento.ConnectionId,
            Score = memento.Score,
            SelectedOptionIndex = memento.SelectedOptionIndex
        };

        foreach (var (questionIndex, answer) in memento.Answers)
        {
            player._answersByQuestionIndex[questionIndex] = answer;
        }

        return player;
    }
}

public sealed record PlayerAnswer(int AnswerIndex, int Score, bool IsCorrect);

/// <summary>
/// Memento：供 Redis/快取序列化用的 Player 狀態快照，讓 Repository 可還原聚合而不破壞封裝。
/// </summary>
public sealed record PlayerMemento(
    Guid Id,
    string Nickname,
    string ConnectionId,
    int Score,
    int? SelectedOptionIndex,
    IReadOnlyDictionary<int, PlayerAnswer> Answers);
