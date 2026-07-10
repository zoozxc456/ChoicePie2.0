using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;

public sealed class InvalidMemberNameException(string name)
    : DomainException(
        internalLogMessage: $"Invalid member name: '{name}'",
        presentationMessage: "顯示名稱長度需介於 2 到 20 字之間。",
        errorCode: "INVALID_MEMBER_NAME",
        statusCode: HttpStatusCode.BadRequest);
