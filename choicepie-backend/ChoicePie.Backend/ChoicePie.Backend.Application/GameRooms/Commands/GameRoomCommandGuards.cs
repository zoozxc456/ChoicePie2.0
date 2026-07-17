using ChoicePie.Backend.Application.GameRooms.Dtos;
using GameRoomAggregate = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoom;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

internal static class GameRoomCommandGuards
{
    public static QuestionPayloadDto CurrentQuestionPayload(GameRoomAggregate room)
    {
        var question = room.Questions[room.CurrentQuestionIndex];
        return new QuestionPayloadDto(
            room.CurrentQuestionIndex,
            room.Questions.Count,
            question.Text,
            question.Options,
            room.TimeLimitSeconds);
    }
}
