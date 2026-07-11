using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.GameRoom.Exceptions;

public sealed class InvalidTimeLimitException(int timeLimitSeconds)
    : DomainException(
        internalLogMessage: $"Time limit {timeLimitSeconds}s is not one of the allowed values (10/20/30/60).",
        presentationMessage: "每題時限必須是 10、20、30 或 60 秒。",
        errorCode: "GAME_ROOM_INVALID_TIME_LIMIT",
        statusCode: HttpStatusCode.BadRequest);
