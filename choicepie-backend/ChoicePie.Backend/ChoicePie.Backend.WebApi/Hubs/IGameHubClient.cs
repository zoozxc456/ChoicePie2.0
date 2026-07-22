using ChoicePie.Backend.Application.GameRooms.Dtos;

namespace ChoicePie.Backend.WebApi.Hubs;

/// <summary>Server → Client 事件，對應 backend-spec.md 第 2.2 節。</summary>
public interface IGameHubClient
{
    Task RoomCreated(string roomCode);
    Task PlayerJoined(PlayerDto player);
    Task PlayerLeft(string connectionId);
    Task AnswerProgress(AnswerProgressDto payload);
    Task GameStarted();
    Task QuestionStart(QuestionPayloadDto payload);
    Task AnswerResult(AnswerResultDto payload);
    Task QuestionEnd(QuestionEndPayloadDto payload);
    Task GameEnd(IReadOnlyList<RankEntryDto> rankings);
    Task RoomStateSync(RoomStateSyncDto payload);
}
