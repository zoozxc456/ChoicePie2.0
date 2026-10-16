using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class WebPageRequiresJavaScriptException(int extractedLength)
    : DomainException(
        internalLogMessage: $"Web page looks client-rendered (app shell); extracted only {extractedLength} chars without JavaScript.",
        presentationMessage: "此網頁需要 JavaScript 才能顯示內容，系統無法擷取，請改為直接貼上文章文字。",
        errorCode: "WEB_PAGE_REQUIRES_JAVASCRIPT",
        statusCode: HttpStatusCode.UnprocessableEntity);
