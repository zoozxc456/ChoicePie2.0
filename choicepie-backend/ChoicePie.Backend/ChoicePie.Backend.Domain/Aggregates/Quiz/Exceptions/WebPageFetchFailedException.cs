using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class WebPageFetchFailedException(string reason, Exception? innerException = null)
    : DomainException(
        internalLogMessage: $"Web page fetch failed: {reason}",
        presentationMessage: "無法讀取此網址，請確認網址可公開存取且內容為網頁或純文字。",
        errorCode: "WEB_PAGE_FETCH_FAILED",
        statusCode: HttpStatusCode.UnprocessableEntity,
        innerException: innerException);
