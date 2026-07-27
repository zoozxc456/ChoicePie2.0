using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;

public sealed class InvalidQuizReportException(string reason)
    : DomainException(
        internalLogMessage: $"Invalid quiz report: {reason}",
        presentationMessage: reason,
        errorCode: "INVALID_QUIZ_REPORT",
        statusCode: HttpStatusCode.BadRequest);
