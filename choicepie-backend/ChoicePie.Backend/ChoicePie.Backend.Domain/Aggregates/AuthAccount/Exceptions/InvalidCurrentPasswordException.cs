using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

// 刻意用 400 而不是 401：前端 useApi 會把 401 當成 access token 過期而觸發 refresh + 重試，
// 「目前密碼輸入錯誤」不是登入狀態失效。
public sealed class InvalidCurrentPasswordException()
    : DomainException(
        internalLogMessage: "Current password verification failed.",
        presentationMessage: "目前密碼不正確。",
        errorCode: "INVALID_CURRENT_PASSWORD",
        statusCode: HttpStatusCode.BadRequest);
