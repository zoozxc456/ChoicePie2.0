using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.QuizReport.Exceptions;

public sealed class QuizReportNotFoundException(Guid reportId)
    : DomainException(
        internalLogMessage: $"QuizReport {reportId} not found.",
        presentationMessage: "找不到指定的檢舉紀錄。",
        errorCode: "QUIZ_REPORT_NOT_FOUND",
        statusCode: HttpStatusCode.NotFound);
