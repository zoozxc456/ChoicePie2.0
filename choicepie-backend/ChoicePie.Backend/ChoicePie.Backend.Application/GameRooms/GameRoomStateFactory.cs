using ChoicePie.Backend.Application.GameRooms.Commands;
using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Entities;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using GameRoomAggregate = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoom;

namespace ChoicePie.Backend.Application.GameRooms;

/// <summary>由 GameRoom 組出 <see cref="RoomStateSyncDto"/>，Host / Player 重連與 JoinRoom 共用。</summary>
internal static class GameRoomStateFactory
{
    public static RoomStateSyncDto Build(GameRoomAggregate room, DateTime utcNow, bool forHost, Player? self = null) =>
        BuildCore(room, utcNow, forHost, self) with { IsHostOnline = room.IsHostOnline };

    private static RoomStateSyncDto BuildCore(GameRoomAggregate room, DateTime utcNow, bool forHost, Player? self)
    {
        var players = room.Players
            .OrderByDescending(p => p.Score)
            .Select((p, index) => forHost
                ? PlayerDto.ForHost(p, index + 1, room.CurrentQuestionIndex)
                : PlayerDto.ForPublic(p, index + 1, room.CurrentQuestionIndex))
            .ToList();

        var roomDto = new GameRoomDto(
            room.RoomCode,
            GameRoomDto.StatusOf(room.Phase),
            players,
            room.CurrentQuestionIndex,
            room.Questions.Count);

        var selfDto = self is null ? null : BuildSelf(room, self);

        if (room.Phase == GamePhase.Question)
        {
            var answeredCount = room.Players.Count(p => p.HasAnsweredQuestion(room.CurrentQuestionIndex));
            return new RoomStateSyncDto(
                room.Phase.Name, roomDto, GameRoomCommandGuards.CurrentQuestionPayload(room),
                answeredCount, room.Players.Count, null, null,
                room.IsPaused, room.GetRemainingMs(utcNow), selfDto);
        }

        if (room.Phase == GamePhase.Reveal)
        {
            var question = room.Questions[room.CurrentQuestionIndex];
            var questionEnd = new QuestionEndPayloadDto(
                question.AnswerIndex, question.Explanation, room.GetRankings().Select(RankEntryDto.FromDomain).ToList());
            return new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, questionEnd, null, Self: selfDto);
        }

        if (room.Phase == GamePhase.Ended)
        {
            var rankings = room.GetRankings().Select(RankEntryDto.FromDomain).ToList();
            return new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, null, rankings, Self: selfDto);
        }

        return new RoomStateSyncDto(room.Phase.Name, roomDto, null, null, null, null, null, Self: selfDto);
    }

    private static PlayerSelfDto BuildSelf(GameRoomAggregate room, Player self)
    {
        var rank = room.GetRankings().First(r => r.PlayerId == self.Id).Rank;
        var index = room.CurrentQuestionIndex;
        var hasAnswered = index >= 0 && self.HasAnsweredQuestion(index);

        AnswerResultDto? lastResult = null;
        if (hasAnswered)
        {
            var answer = self.Answers[index];
            lastResult = new AnswerResultDto(answer.IsCorrect, room.Questions[index].AnswerIndex, answer.Score);
        }

        return new PlayerSelfDto(
            self.Id, self.Nickname, self.Score, rank, hasAnswered, self.SelectedOptionIndex, lastResult);
    }
}
