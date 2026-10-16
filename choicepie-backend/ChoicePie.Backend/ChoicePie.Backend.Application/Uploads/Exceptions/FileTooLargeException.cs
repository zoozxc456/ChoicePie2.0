using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Application.Uploads.Exceptions;

public sealed class FileTooLargeException(long actualBytes, long maxBytes)
    : DomainException(
        internalLogMessage: $"Uploaded file too large: {actualBytes} bytes (max {maxBytes}).",
        presentationMessage: "圖片檔案過大，請上傳 5MB 以內的圖片。",
        errorCode: "FILE_TOO_LARGE",
        statusCode: HttpStatusCode.BadRequest);
