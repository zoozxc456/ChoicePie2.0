using System.Net;

namespace ChoicePie.Backend.Shared.Kernel.Exceptions;

/// <summary>
/// 同時進來的請求都通過了「是否已存在」的檢查，最後由資料庫唯一索引擋下其中之一。
/// 預設回 409；handler 可以捕捉後改成冪等成功或更貼切的業務例外。
/// </summary>
public sealed class UniqueConstraintViolationException(string? constraintName, Exception? innerException = null)
    : DomainException(
        internalLogMessage: $"Unique constraint violated: {constraintName}",
        presentationMessage: "資料已存在，請重新整理後再試。",
        errorCode: "DUPLICATE_RESOURCE",
        statusCode: HttpStatusCode.Conflict,
        innerException: innerException)
{
    public string? ConstraintName { get; } = constraintName;
}
