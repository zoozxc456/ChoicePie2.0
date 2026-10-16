namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>重連玩家本人的狀態：自己的分數/名次，以及是否已作答當前題目（含已作答的結果，供還原 UI）。</summary>
public sealed record PlayerSelfDto(
    Guid PlayerId,
    string Nickname,
    int Score,
    int Rank,
    bool HasAnswered,
    int? SelectedOptionIndex,
    AnswerResultDto? LastAnswerResult);
