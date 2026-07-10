using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class InvalidQuizException(string reason)
    : DomainException(
        internalLogMessage: $"Invalid quiz: {reason}",
        presentationMessage: reason,
        errorCode: "INVALID_QUIZ",
        statusCode: HttpStatusCode.BadRequest);
