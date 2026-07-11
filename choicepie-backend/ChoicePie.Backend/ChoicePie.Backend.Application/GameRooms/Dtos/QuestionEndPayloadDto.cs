namespace ChoicePie.Backend.Application.GameRooms.Dtos;

public sealed record QuestionEndPayloadDto(
    int AnswerIndex,
    string Explanation,
    IReadOnlyList<RankEntryDto> Rankings);
