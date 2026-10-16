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
    private static readonly TimeSpan ViewableWindow = TimeSpan.FromHours(48);

    public Guid HostUserId { get; private set; }
    public string RoomCode { get; private set; } = null!;
    public Guid QuizId { get; private set; }
    public string QuizTitle { get; private set; } = null!;
    public string? CoverImageUrl { get; private set; }
    public string CoverEmoji { get; private set; } = null!;
    public string CoverGradient { get; private set; } = null!;
    public int TimeLimitSeconds { get; private set; }
    public GamePhase Phase { get; private set; } = null!;
    public int CurrentQuestionIndex { get; private set; } = -1;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CurrentQuestionStartedAtUtc { get; private set; }
    public TimeSpan PausedAccumulated { get; private set; } = TimeSpan.Zero;
    public DateTime? PausedAtUtc { get; private set; }

    /// <summary>Host 目前使用中的 SignalR 連線 Id；用來辨識過期（舊分頁）連線的斷線通知。</summary>
    public string? HostConnectionId { get; private set; }

    public bool IsHostOnline { get; private set; } = true;

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
        string? coverImageUrl,
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
            CoverImageUrl = coverImageUrl,
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

        var trimmed = nickname.Trim();
        if (_players.Any(p => string.Equals(p.Nickname.Trim(), trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            throw new DuplicateNicknameException(RoomCode, trimmed);
        }

        var player = Player.Create(trimmed, connectionId, memberId);
        _players.Add(player);

        return player;
    }

    public void StartGame(Guid callerUserId, DateTime utcNow)
    {
        EnsureHost(callerUserId);
        EnsurePhase(GamePhase.Lobby);

        Phase = GamePhase.Question;
        CurrentQuestionIndex = 0;
        CurrentQuestionStartedAtUtc = utcNow;
    }

    public AnswerOutcome SubmitAnswer(Guid playerId, int answerIndex, DateTime answeredAtUtc)
    {
        EnsurePhase(GamePhase.Question);

        if (IsPaused)
        {
            throw new GamePausedException(RoomCode);
        }

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

        player.RecordAnswer(CurrentQuestionIndex, answerIndex, score, isCorrect, (long)elapsed.TotalMilliseconds);

        return new AnswerOutcome(score, isCorrect, question.AnswerIndex);
    }

    public IReadOnlyList<RankEntrySnapshot> EndCurrentQuestion(Guid callerUserId, DateTime utcNow)
    {
        EnsureHost(callerUserId);
        EnsurePhase(GamePhase.Question);

        if (PausedAtUtc is not null)
        {
            PausedAccumulated += utcNow - PausedAtUtc.Value;
            PausedAtUtc = null;
        }

        Phase = GamePhase.Reveal;

        return BuildRankings();
    }

    public bool AdvanceToNextQuestion(Guid callerUserId, DateTime utcNow)
    {
        EnsureHost(callerUserId);
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

    public void TogglePause(Guid callerUserId, DateTime utcNow)
    {
        EnsureHost(callerUserId);

        if (PausedAtUtc is null)
        {
            EnsurePhase(GamePhase.Question);
            PausedAtUtc = utcNow;
            return;
        }

        PausedAccumulated += utcNow - PausedAtUtc.Value;
        PausedAtUtc = null;
    }

    public bool IsPaused => PausedAtUtc is not null;

    /// <summary>Host 建立房間或重連：記下目前連線並標記上線。不會自動繼續遊戲，需由 Host 手動按繼續。</summary>
    public void AttachHostConnection(Guid callerUserId, string connectionId)
    {
        EnsureHost(callerUserId);

        HostConnectionId = connectionId;
        IsHostOnline = true;
    }

    /// <summary>
    /// Host 的連線中斷。只有「目前」的 Host 連線才處理（舊分頁的斷線通知忽略）。
    /// 題目進行中且尚未暫停時，沿用手動暫停的同一套邏輯自動暫停，讓計分排除離線時間。
    /// </summary>
    public HostDisconnectOutcome DisconnectHost(string connectionId, DateTime utcNow)
    {
        if (!IsHostOnline || HostConnectionId != connectionId)
        {
            return HostDisconnectOutcome.Ignored;
        }

        IsHostOnline = false;

        if (Phase == GamePhase.Question && !IsPaused)
        {
            PausedAtUtc = utcNow;
            return HostDisconnectOutcome.MarkedOfflineAndPaused;
        }

        return HostDisconnectOutcome.MarkedOffline;
    }

    /// <summary>目前題目的剩餘毫秒數（伺服器計算，暫停期間凍結、排除暫停累計時間）。</summary>
    public long GetRemainingMs(DateTime utcNow)
    {
        if (CurrentQuestionStartedAtUtc is null)
        {
            return 0;
        }

        var reference = PausedAtUtc ?? utcNow;
        var elapsed = reference - CurrentQuestionStartedAtUtc.Value - PausedAccumulated;
        var remaining = TimeSpan.FromSeconds(TimeLimitSeconds) - elapsed;

        return Math.Clamp((long)remaining.TotalMilliseconds, 0, (long)TimeSpan.FromSeconds(TimeLimitSeconds).TotalMilliseconds);
    }

    /// <summary>
    /// 玩家連線中斷：大廳階段直接移除以釋出名額；遊戲開始後只標記離線（保留分數與作答、仍列入排名與 GameSession）。
    /// 只有當傳入的 connectionId 仍是該玩家目前的連線時才處理，避免「快速重連後舊連線的斷線通知」把新連線誤判為離線。
    /// </summary>
    public PlayerDisconnectOutcome DisconnectPlayer(Guid playerId, string connectionId)
    {
        var player = _players.SingleOrDefault(p => p.Id == playerId);
        if (player is null || player.ConnectionId != connectionId)
        {
            return PlayerDisconnectOutcome.Ignored;
        }

        if (Phase == GamePhase.Lobby)
        {
            _players.Remove(player);
            return PlayerDisconnectOutcome.Removed;
        }

        player.MarkDisconnected();
        return PlayerDisconnectOutcome.MarkedDisconnected;
    }

    /// <summary>以重連憑證找回玩家並換上新的連線 Id。</summary>
    public Player ReconnectPlayer(string? reconnectToken, string newConnectionId)
    {
        var player = _players.FirstOrDefault(p => p.MatchesReconnectToken(reconnectToken))
                     ?? throw new InvalidReconnectTokenException(RoomCode);

        player.Reconnect(newConnectionId);

        return player;
    }

    /// <summary>旁觀（大螢幕）用：房間過了保存期限（48h，與快取清除時間一致）就不能再看。</summary>
    public void EnsureViewable(DateTime utcNow)
    {
        if (utcNow >= CreatedAtUtc + ViewableWindow)
        {
            throw new RoomNotJoinableException(RoomCode);
        }
    }

    public IReadOnlyList<RankEntrySnapshot> GetRankings() => BuildRankings();

    public void EnsureHost(Guid callerUserId)
    {
        if (HostUserId != callerUserId)
        {
            throw new RoomAccessDeniedException(RoomCode);
        }
    }

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
            CoverImageUrl,
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
            _players.Select(p => p.ToMemento()).ToList(),
            HostConnectionId,
            IsHostOnline);

    public static GameRoom Restore(GameRoomMemento memento)
    {
        var room = new GameRoom
        {
            Id = memento.Id,
            HostUserId = memento.HostUserId,
            RoomCode = memento.RoomCode,
            QuizId = memento.QuizId,
            QuizTitle = memento.QuizTitle,
            CoverImageUrl = memento.CoverImageUrl,
            CoverEmoji = memento.CoverEmoji,
            CoverGradient = memento.CoverGradient,
            TimeLimitSeconds = memento.TimeLimitSeconds,
            Phase = GamePhase.FromName(memento.Phase) ?? throw new InvalidGameRoomException($"未知的遊戲階段：{memento.Phase}"),
            CurrentQuestionIndex = memento.CurrentQuestionIndex,
            CreatedAtUtc = memento.CreatedAtUtc,
            CurrentQuestionStartedAtUtc = memento.CurrentQuestionStartedAtUtc,
            PausedAccumulated = memento.PausedAccumulated,
            PausedAtUtc = memento.PausedAtUtc,
            HostConnectionId = memento.HostConnectionId,
            IsHostOnline = memento.IsHostOnline
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
    string? CoverImageUrl,
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
    IReadOnlyList<PlayerMemento> Players,
    string? HostConnectionId = null,
    bool IsHostOnline = true);
