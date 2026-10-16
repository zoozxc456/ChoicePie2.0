using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class WebPageContentInsufficientException(int extractedLength)
    : DomainException(
        internalLogMessage: $"Extracted web page text too short: {extractedLength} chars.",
        presentationMessage: "無法從此網址擷取足夠的文字內容，請改貼上文字內容。",
        errorCode: "WEB_PAGE_CONTENT_INSUFFICIENT",
        statusCode: HttpStatusCode.UnprocessableEntity);
