using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Application.Uploads.Exceptions;

public sealed class InvalidFileTypeException(string? contentType)
    : DomainException(
        internalLogMessage: $"Invalid uploaded file content type: {contentType}",
        presentationMessage: "不支援的圖片格式，僅接受 JPEG、PNG、WebP。",
        errorCode: "INVALID_FILE_TYPE",
        statusCode: HttpStatusCode.BadRequest);
