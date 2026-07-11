using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class InvalidGameRoomException(string reason)
    : DomainException(
        internalLogMessage: $"Invalid game room: {reason}",
        presentationMessage: reason,
        errorCode: "GAME_ROOM_INVALID",
        statusCode: HttpStatusCode.BadRequest);
