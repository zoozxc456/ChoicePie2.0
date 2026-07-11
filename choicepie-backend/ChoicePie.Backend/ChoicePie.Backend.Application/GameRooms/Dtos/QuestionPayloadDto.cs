namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>
/// 推給 Player/Bigscreen 的題目內容——刻意不含 AnswerIndex/Explanation，避免正解提前外流。
/// </summary>
public sealed record QuestionPayloadDto(
    int Index,
    int Total,
    string Text,
    IReadOnlyList<string> Options,
    int TimeLimit);
