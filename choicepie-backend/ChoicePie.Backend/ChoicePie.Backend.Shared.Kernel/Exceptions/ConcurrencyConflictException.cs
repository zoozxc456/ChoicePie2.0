using System.Net;

namespace ChoicePie.Backend.Shared.Kernel.Exceptions;

/// <summary>
/// 存檔時發現資料在讀取之後已被其他請求修改（樂觀並行控制失敗）。
/// 預設回 409；需要不同語意的 handler（例如一次性 token 已被用掉）可以捕捉後改丟更貼切的例外。
/// </summary>
public sealed class ConcurrencyConflictException(Exception? innerException = null)
    : DomainException(
        internalLogMessage: "Optimistic concurrency conflict: the row was modified after it was loaded.",
        presentationMessage: "資料已被其他操作更新，請重新整理後再試。",
        errorCode: "CONCURRENCY_CONFLICT",
        statusCode: HttpStatusCode.Conflict,
        innerException: innerException);
