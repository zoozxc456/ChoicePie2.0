using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

public sealed class WebPageAccessDeniedException(string reason)
    : DomainException(
        internalLogMessage: $"Web page access denied: {reason}",
        presentationMessage: "此網址不允許存取，請改用公開的網頁網址。",
        errorCode: "WEB_PAGE_ACCESS_DENIED",
        statusCode: HttpStatusCode.BadRequest);
