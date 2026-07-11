namespace ChoicePie.Backend.Application.GameRooms.Dtos;

public sealed record AnswerResultDto(bool IsCorrect, int CorrectAnswerIndex, int PointsEarned);
