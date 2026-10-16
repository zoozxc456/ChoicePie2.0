using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class UnsupportedWebPageUrlException(string reason)
    : DomainException(
        internalLogMessage: $"Unsupported web page URL: {reason}",
        presentationMessage: "網址格式不支援，僅支援公開的 http / https 網頁網址。",
        errorCode: "WEB_PAGE_URL_UNSUPPORTED",
        statusCode: HttpStatusCode.BadRequest);
