namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>重新連線者（Host）用來還原到正確 GamePhase 的完整房間快照。</summary>
public sealed record RoomStateSyncDto(
    string Phase,
    GameRoomDto Room,
    QuestionPayloadDto? CurrentQuestion,
    int? AnsweredCount,
    int? TotalCount,
    QuestionEndPayloadDto? QuestionEnd,
    IReadOnlyList<RankEntryDto>? Rankings);
