using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public enum SkipQuestionOutcomeKind
{
    QuestionEnded,
    AdvancedToNextQuestion,
    GameEnded
}

public sealed record SkipQuestionResultDto(
    SkipQuestionOutcomeKind Kind,
    QuestionEndPayloadDto? QuestionEnd,
    QuestionPayloadDto? NextQuestion,
    IReadOnlyList<RankEntryDto>? FinalRankings);

/// <summary>
/// 對應 Hub 的 SkipQuestion：Question 階段呼叫會提前結束該題（進入 Reveal）；
/// Reveal 階段呼叫則代表 Host 看完結果按下一題，推進到下一題或結束遊戲。
/// spec 只定義了「跳過目前題目」這一個 Host 動作，沒有另外的「下一題」RPC，因此借用同一個方法涵蓋兩種情境。
/// </summary>
public sealed record SkipQuestionCommand(string RoomCode, Guid HostUserId) : IRequest<SkipQuestionResultDto>;
