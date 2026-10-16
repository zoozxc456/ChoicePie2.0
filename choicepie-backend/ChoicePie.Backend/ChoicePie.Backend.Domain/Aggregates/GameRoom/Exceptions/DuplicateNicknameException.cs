using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class DuplicateNicknameException(string roomCode, string nickname)
    : DomainException(
        internalLogMessage: $"Nickname '{nickname}' already used in room {roomCode}.",
        presentationMessage: "此暱稱已有人使用，請換一個暱稱。",
        errorCode: "GAME_ROOM_DUPLICATE_NICKNAME",
        statusCode: HttpStatusCode.Conflict);
