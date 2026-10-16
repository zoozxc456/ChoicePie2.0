using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class WebPageFetchTimeoutException()
    : DomainException(
        internalLogMessage: "Web page fetch timed out.",
        presentationMessage: "讀取此網址逾時，請稍後再試或改貼上文字內容。",
        errorCode: "WEB_PAGE_FETCH_TIMEOUT",
        statusCode: HttpStatusCode.UnprocessableEntity);
