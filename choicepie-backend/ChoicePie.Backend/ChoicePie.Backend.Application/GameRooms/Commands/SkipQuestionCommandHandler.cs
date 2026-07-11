using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.GameSession;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class SkipQuestionCommandHandler(
    IGameRoomRepository gameRoomRepository,
    IGameSessionRepository gameSessionRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SkipQuestionCommand, SkipQuestionResultDto>
{
    public async Task<SkipQuestionResultDto> Handle(SkipQuestionCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        GameRoomCommandGuards.EnsureHost(room, request.HostUserId);

        var now = DateTime.UtcNow;

        var result = room.Phase == GamePhase.Question
            ? EndCurrentQuestion(room, now)
            : await AdvanceAsync(room, now, cancellationToken);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        return result;
    }

    private static SkipQuestionResultDto EndCurrentQuestion(Domain.Aggregates.GameRoom.GameRoom room, DateTime now)
    {
        var rankings = room.EndCurrentQuestion(now);
        var question = room.Questions[room.CurrentQuestionIndex];

        var questionEnd = new QuestionEndPayloadDto(
            question.AnswerIndex,
            question.Explanation,
            rankings.Select(RankEntryDto.FromDomain).ToList());

        return new SkipQuestionResultDto(SkipQuestionOutcomeKind.QuestionEnded, questionEnd, null, null);
    }

    private async Task<SkipQuestionResultDto> AdvanceAsync(
        Domain.Aggregates.GameRoom.GameRoom room, DateTime now, CancellationToken ct)
    {
        var ended = room.AdvanceToNextQuestion(now);

        if (!ended)
        {
            return new SkipQuestionResultDto(
                SkipQuestionOutcomeKind.AdvancedToNextQuestion, null, GameRoomCommandGuards.CurrentQuestionPayload(room), null);
        }

        var session = GameSessionAggregate.RecordFrom(room, now);
        await gameSessionRepository.AddAsync(session, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var finalRankings = session.PlayerResults
            .Select(r => new RankEntryDto(r.Rank, r.Nickname, r.FinalScore))
            .OrderBy(r => r.Rank)
            .ToList();

        return new SkipQuestionResultDto(SkipQuestionOutcomeKind.GameEnded, null, null, finalRankings);
    }
}
