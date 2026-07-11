using ChoicePie.Backend.Application.GameRooms.Dtos;
using ChoicePie.Backend.Domain.Aggregates.GameRoom;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed class SubmitAnswerCommandHandler(IGameRoomRepository gameRoomRepository)
    : IRequestHandler<SubmitAnswerCommand, SubmitAnswerResultDto>
{
    public async Task<SubmitAnswerResultDto> Handle(SubmitAnswerCommand request, CancellationToken cancellationToken)
    {
        var room = await gameRoomRepository.GetByRoomCodeAsync(request.RoomCode, cancellationToken)
                   ?? throw new RoomNotFoundException(request.RoomCode);

        // 得分完全由後端依時間制計分表計算，前端只送 answerIndex，避免作弊。
        var outcome = room.SubmitAnswer(request.PlayerId, request.AnswerIndex, DateTime.UtcNow);

        await gameRoomRepository.SaveAsync(room, cancellationToken);

        var result = new AnswerResultDto(outcome.IsCorrect, outcome.CorrectAnswerIndex, outcome.Score);
        var answeredCount = room.Players.Count(p => p.HasAnsweredQuestion(room.CurrentQuestionIndex));
        var progress = new AnswerProgressDto(answeredCount, room.Players.Count, request.PlayerId, request.AnswerIndex);

        return new SubmitAnswerResultDto(result, progress);
    }
}
