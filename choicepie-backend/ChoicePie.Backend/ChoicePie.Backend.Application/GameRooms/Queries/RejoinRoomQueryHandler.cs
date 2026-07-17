using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Queries;

public sealed class RejoinRoomQueryHandler(IGameRoomRepository gameRoomRepository)
    : IRequestHandler<RejoinRoomQuery, RoomStateSyncDto>
{
    public async Task<RoomStateSyncDto> Handle(RejoinRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        room.EnsureHost(request.HostUserId);

        var players = room.Players
            .OrderByDescending(p => p.Score)
            .Select((p, index) => PlayerDto.ForHost(p, index + 1, room.CurrentQuestionIndex))
            .ToList();

        var roomDto = new GameRoomDto(
            room.RoomCode,
            GameRoomDto.StatusOf(room.Phase),
            players,
            room.CurrentQuestionIndex,
            room.Questions.Count);

        if (room.Phase == GamePhase.Question)
        {
            var answeredCount = room.Players.Count(p => p.HasAnsweredQuestion(room.CurrentQuestionIndex));
            return new RoomStateSyncDto(
                room.Phase.Name, roomDto, GameRoomCommandGuards.CurrentQuestionPayload(room),
                answeredCount, room.Players.Count, null, null);
        }

        if (room.Phase == GamePhase.Reveal)
        {
            var question = room.Questions[room.CurrentQuestionIndex];
            var questionEnd = new QuestionEndPayloadDto(
                question.AnswerIndex, question.Explanation, room.GetRankings().Select(RankEntryDto.FromDomain).ToList());
            return new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, questionEnd, null);
        }

        if (room.Phase == GamePhase.Ended)
        {
            var rankings = room.GetRankings().Select(RankEntryDto.FromDomain).ToList();
            return new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, null, rankings);
        }

        return new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, null, null);
    }
}
