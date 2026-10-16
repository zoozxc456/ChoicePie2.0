namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>重新連線者（Host / Player）用來還原到正確 GamePhase 的完整房間快照。</summary>
public sealed record RoomStateSyncDto(
    string Phase,
    GameRoomDto Room,
    QuestionPayloadDto? CurrentQuestion,
    int? AnsweredCount,
    int? TotalCount,
    QuestionEndPayloadDto? QuestionEnd,
    IReadOnlyList<RankEntryDto>? Rankings,
    bool IsPaused = false,
    long? RemainingMs = null,
    PlayerSelfDto? Self = null,
    bool IsHostOnline = true);
