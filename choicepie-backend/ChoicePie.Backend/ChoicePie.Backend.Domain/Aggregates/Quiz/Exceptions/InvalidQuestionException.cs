using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class InvalidQuestionException(string reason)
    : DomainException(
        internalLogMessage: $"Invalid question: {reason}",
        presentationMessage: reason,
        errorCode: "INVALID_QUESTION",
        statusCode: HttpStatusCode.BadRequest);
