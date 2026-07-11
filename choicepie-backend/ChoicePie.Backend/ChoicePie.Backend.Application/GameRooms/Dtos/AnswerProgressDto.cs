namespace ChoicePie.Backend.Application.GameRooms.Dtos;

/// <summary>Host-only：即時作答進度，含所選選項，用於即時統計。</summary>
public sealed record AnswerProgressDto(int Answered, int Total, Guid PlayerId, int SelectedOptionIndex);
