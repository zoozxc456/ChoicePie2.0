using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Entities;

public sealed class Player : AuditableEntity<Guid>
{
    private readonly Dictionary<int, PlayerAnswer> _answersByQuestionIndex = [];

    public string Nickname { get; private set; } = null!;
    public string ConnectionId { get; private set; } = null!;
    public Guid? MemberId { get; private set; }
    public int Score { get; private set; }
    public int? SelectedOptionIndex { get; private set; }

    private Player()
    {
    }

    /// <summary>
    /// memberId 為選填：玩家不需登入即可加入房間，但若加入時已登入，會記下 MemberId
    /// 讓遊戲結束後的 GameSession 快照可歸戶到該使用者的「我玩過的遊戲」歷史。
    /// </summary>
    public static Player Create(string nickname, string connectionId, Guid? memberId = null) => new()
    {
        Id = Guid.NewGuid(),
        Nickname = nickname,
        ConnectionId = connectionId,
        MemberId = memberId
    };

    public IReadOnlyDictionary<int, PlayerAnswer> Answers => _answersByQuestionIndex;

    public bool HasAnsweredQuestion(int questionIndex) => _answersByQuestionIndex.ContainsKey(questionIndex);

    public void RecordAnswer(int questionIndex, int answerIndex, int score, bool isCorrect, long answerTimeMs = 0)
    {
        _answersByQuestionIndex[questionIndex] = new PlayerAnswer(answerIndex, score, isCorrect, answerTimeMs);
        SelectedOptionIndex = answerIndex;
        Score += score;
    }

    public void ResetForNextQuestion() => SelectedOptionIndex = null;

    public PlayerMemento ToMemento() =>
        new(Id, Nickname, ConnectionId, MemberId, Score, SelectedOptionIndex, _answersByQuestionIndex);

    public static Player Restore(PlayerMemento memento)
    {
        var player = new Player
        {
            Id = memento.Id,
            Nickname = memento.Nickname,
            ConnectionId = memento.ConnectionId,
            MemberId = memento.MemberId,
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

public sealed record PlayerAnswer(int AnswerIndex, int Score, bool IsCorrect, long AnswerTimeMs = 0);

/// <summary>
/// Memento：供 Redis/快取序列化用的 Player 狀態快照，讓 Repository 可還原聚合而不破壞封裝。
/// </summary>
public sealed record PlayerMemento(
    Guid Id,
    string Nickname,
    string ConnectionId,
    Guid? MemberId,
    int Score,
    int? SelectedOptionIndex,
    IReadOnlyDictionary<int, PlayerAnswer> Answers);
