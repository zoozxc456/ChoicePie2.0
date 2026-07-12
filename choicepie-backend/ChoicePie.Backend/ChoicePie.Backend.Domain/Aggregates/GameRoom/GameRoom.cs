using ChoicePie.Backend.Domain.Aggregates.GameRoom.Entities;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Events;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Services;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom;

public sealed class GameRoom : AggregateRoot<Guid>
{
    private static readonly int[] AllowedTimeLimitsSeconds = [10, 20, 30, 60];

    private readonly List<Player> _players = [];
    private readonly List<GameQuestionSnapshot> _questions = [];

    private const int MaxPlayers = 30;
    private static readonly TimeSpan PlayableWindow = TimeSpan.FromHours(24);

    public Guid HostUserId { get; private set; }
    public string RoomCode { get; private set; } = null!;
    public Guid QuizId { get; private set; }
    public string QuizTitle { get; private set; } = null!;
    public string CoverEmoji { get; private set; } = null!;
    public string CoverGradient { get; private set; } = null!;
    public int TimeLimitSeconds { get; private set; }
    public GamePhase Phase { get; private set; } = null!;
    public int CurrentQuestionIndex { get; private set; } = -1;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CurrentQuestionStartedAtUtc { get; private set; }
    public TimeSpan PausedAccumulated { get; private set; } = TimeSpan.Zero;
    public DateTime? PausedAtUtc { get; private set; }

    public IReadOnlyList<Player> Players => _players.AsReadOnly();
    public IReadOnlyList<GameQuestionSnapshot> Questions => _questions.AsReadOnly();

    private GameRoom()
    {
    }

    public static GameRoom Create(
        Guid hostUserId,
        string roomCode,
        Guid quizId,
        string quizTitle,
        string coverEmoji,
        string coverGradient,
        IReadOnlyList<GameQuestionSnapshot> questions,
        int timeLimitSeconds,
        DateTime createdAtUtc)
    {
        if (questions.Count == 0)
        {
            throw new InvalidGameRoomException("題目清單不能為空。");
        }

        if (!AllowedTimeLimitsSeconds.Contains(timeLimitSeconds))
        {
            throw new InvalidTimeLimitException(timeLimitSeconds);
        }

        var room = new GameRoom
        {
            Id = Guid.NewGuid(),
            HostUserId = hostUserId,
            RoomCode = roomCode,
            QuizId = quizId,
            QuizTitle = quizTitle,
            CoverEmoji = coverEmoji,
            CoverGradient = coverGradient,
            TimeLimitSeconds = timeLimitSeconds,
            Phase = GamePhase.Lobby,
            CreatedAtUtc = createdAtUtc
        };

        room._questions.AddRange(questions);

        return room;
    }

    public Player Join(string nickname, string connectionId, DateTime utcNow, Guid? memberId = null)
    {
        if (utcNow >= CreatedAtUtc + PlayableWindow)
        {
            throw new RoomNotJoinableException(RoomCode);
        }

        if (Phase != GamePhase.Lobby)
        {
            throw new RoomAlreadyStartedException(RoomCode);
        }

        if (_players.Count >= MaxPlayers)
        {
            throw new RoomFullException(RoomCode);
        }

        var player = Player.Create(nickname, connectionId, memberId);
        _players.Add(player);

        return player;
    }

    public void StartGame(DateTime utcNow)
    {
        EnsurePhase(GamePhase.Lobby);

        Phase = GamePhase.Question;
        CurrentQuestionIndex = 0;
        CurrentQuestionStartedAtUtc = utcNow;
    }

    public AnswerOutcome SubmitAnswer(Guid playerId, int answerIndex, DateTime answeredAtUtc)
    {
        EnsurePhase(GamePhase.Question);

        var player = _players.SingleOrDefault(p => p.Id == playerId)
                     ?? throw new PlayerNotFoundException(playerId, RoomCode);

        if (player.HasAnsweredQuestion(CurrentQuestionIndex))
        {
            throw new AnswerAlreadySubmittedException(playerId, CurrentQuestionIndex);
        }

        var question = _questions[CurrentQuestionIndex];
        var isCorrect = answerIndex == question.AnswerIndex;
        var elapsed = answeredAtUtc - CurrentQuestionStartedAtUtc!.Value - PausedAccumulated;
        var score = ScoreCalculator.Calculate(elapsed, TimeSpan.FromSeconds(TimeLimitSeconds), isCorrect);

        player.RecordAnswer(CurrentQuestionIndex, answerIndex, score, isCorrect);

        return new AnswerOutcome(score, isCorrect, question.AnswerIndex);
    }

    public IReadOnlyList<RankEntrySnapshot> EndCurrentQuestion(DateTime utcNow)
    {
        EnsurePhase(GamePhase.Question);

        Phase = GamePhase.Reveal;

        return BuildRankings();
    }

    public bool AdvanceToNextQuestion(DateTime utcNow)
    {
        EnsurePhase(GamePhase.Reveal);

        if (CurrentQuestionIndex + 1 < _questions.Count)
        {
            CurrentQuestionIndex++;
            Phase = GamePhase.Question;
            CurrentQuestionStartedAtUtc = utcNow;
            PausedAccumulated = TimeSpan.Zero;
            PausedAtUtc = null;
            foreach (var player in _players)
            {
                player.ResetForNextQuestion();
            }

            return false;
        }

        Phase = GamePhase.Ended;
        AddDomainEvent(new GameEndedDomainEvent(Id, RoomCode, BuildRankings(), utcNow));

        return true;
    }

    public void TogglePause(DateTime utcNow)
    {
        if (PausedAtUtc is null)
        {
            EnsurePhase(GamePhase.Question);
            PausedAtUtc = utcNow;
            return;
        }

        PausedAccumulated += utcNow - PausedAtUtc.Value;
        PausedAtUtc = null;
    }

    public IReadOnlyList<RankEntrySnapshot> GetRankings() => BuildRankings();

    private void EnsurePhase(GamePhase expected)
    {
        if (Phase != expected)
        {
            throw new InvalidGamePhaseException(Phase, expected);
        }
    }

    private IReadOnlyList<RankEntrySnapshot> BuildRankings() =>
        _players
            .OrderByDescending(p => p.Score)
            .Select((p, index) => new RankEntrySnapshot(p.Id, p.Nickname, p.Score, index + 1))
            .ToList();

    public GameRoomMemento ToMemento() =>
        new(
            Id,
            HostUserId,
            RoomCode,
            QuizId,
            QuizTitle,
            CoverEmoji,
            CoverGradient,
            TimeLimitSeconds,
            Phase.Name,
            CurrentQuestionIndex,
            CreatedAtUtc,
            CurrentQuestionStartedAtUtc,
            PausedAccumulated,
            PausedAtUtc,
            _questions,
            _players.Select(p => p.ToMemento()).ToList());

    public static GameRoom Restore(GameRoomMemento memento)
    {
        var room = new GameRoom
        {
            Id = memento.Id,
            HostUserId = memento.HostUserId,
            RoomCode = memento.RoomCode,
            QuizId = memento.QuizId,
            QuizTitle = memento.QuizTitle,
            CoverEmoji = memento.CoverEmoji,
            CoverGradient = memento.CoverGradient,
            TimeLimitSeconds = memento.TimeLimitSeconds,
            Phase = GamePhase.FromName(memento.Phase) ?? throw new InvalidGameRoomException($"未知的遊戲階段：{memento.Phase}"),
            CurrentQuestionIndex = memento.CurrentQuestionIndex,
            CreatedAtUtc = memento.CreatedAtUtc,
            CurrentQuestionStartedAtUtc = memento.CurrentQuestionStartedAtUtc,
            PausedAccumulated = memento.PausedAccumulated,
            PausedAtUtc = memento.PausedAtUtc
        };

        room._questions.AddRange(memento.Questions);
        room._players.AddRange(memento.Players.Select(Player.Restore));

        return room;
    }
}

/// <summary>
/// Memento：供 Redis/快取序列化用的 GameRoom 狀態快照，讓 Repository 可還原聚合而不破壞封裝。
/// </summary>
public sealed record GameRoomMemento(
    Guid Id,
    Guid HostUserId,
    string RoomCode,
    Guid QuizId,
    string QuizTitle,
    string CoverEmoji,
    string CoverGradient,
    int TimeLimitSeconds,
    string Phase,
    int CurrentQuestionIndex,
    DateTime CreatedAtUtc,
    DateTime? CurrentQuestionStartedAtUtc,
    TimeSpan PausedAccumulated,
    DateTime? PausedAtUtc,
    IReadOnlyList<GameQuestionSnapshot> Questions,
    IReadOnlyList<PlayerMemento> Players);
