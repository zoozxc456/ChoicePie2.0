using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;

namespace ChoicePie.Backend.Application.GameRooms.Dtos;

public sealed record GameRoomDto(
    string RoomCode,
    string Status,
    IReadOnlyList<PlayerDto> Players,
    int CurrentQuestionIndex,
    int TotalQuestions)
{
    public static string StatusOf(GamePhase phase) => phase.Name switch
    {
        "lobby" => "waiting",
        "ended" => "ended",
        _ => "playing"
    };
}
