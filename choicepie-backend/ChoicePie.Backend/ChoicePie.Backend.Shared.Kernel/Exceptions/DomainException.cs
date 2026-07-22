using System.Net;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Shared.Kernel.Exceptions;

/// <summary>
/// 所有業務邏輯異常的基底類別
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// 給前端/使用者看的訊息 (安全、經過修飾、可本地化)
    /// </summary>
    public string UserFriendlyMessage { get; }

    /// <summary>
    /// 錯誤代碼 (可方便前端做多語系或邏輯判斷)
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// HTTP Status Code
    /// </summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Exception 的 LogLevel
    /// </summary>
    public LogLevel LogLevel;

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="internalLogMessage">給開發人員看的 Log (可能包含敏感數據，絕對不要回傳給前端)</param>
    /// <param name="presentationMessage">給使用者看的訊息</param>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="statusCode">HTTP Status Code</param>
    /// <param name="logLevel">Exception 的 LogLevel</param>
    /// <param name="innerException">原始異常</param>
    protected DomainException(
        string internalLogMessage,
        string presentationMessage,
        string errorCode = "DOMAIN_ERROR",
        HttpStatusCode statusCode = HttpStatusCode.BadRequest,
        LogLevel logLevel = LogLevel.Warning,
        Exception? innerException = null)
        : base(internalLogMessage, innerException)
    {
        UserFriendlyMessage = presentationMessage;
        ErrorCode = errorCode;
        StatusCode = statusCode;
        LogLevel = logLevel;
    }
}
