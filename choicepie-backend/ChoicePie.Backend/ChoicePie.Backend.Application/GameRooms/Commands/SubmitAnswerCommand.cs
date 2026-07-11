using ChoicePie.Backend.Application.GameRooms.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.GameRooms.Commands;

public sealed record SubmitAnswerResultDto(AnswerResultDto Result, AnswerProgressDto Progress);

public sealed record SubmitAnswerCommand(string RoomCode, Guid PlayerId, int AnswerIndex) : IRequest<SubmitAnswerResultDto>;
