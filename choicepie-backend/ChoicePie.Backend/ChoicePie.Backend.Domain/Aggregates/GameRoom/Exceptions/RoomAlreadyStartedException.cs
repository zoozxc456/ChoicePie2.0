using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class RoomAlreadyStartedException(string roomCode)
    : DomainException(
        internalLogMessage: $"Room {roomCode} has already started; late join is not allowed.",
        presentationMessage: "遊戲已經開始，無法加入。",
        errorCode: "GAME_ROOM_ALREADY_STARTED",
        statusCode: HttpStatusCode.Conflict);
