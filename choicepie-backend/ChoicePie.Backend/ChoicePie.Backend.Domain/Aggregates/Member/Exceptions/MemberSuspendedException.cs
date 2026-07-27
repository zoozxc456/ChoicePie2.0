using System.Net;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;

public sealed class MemberSuspendedException(Guid memberId, string? reason)
    : DomainException(
        internalLogMessage: $"Member {memberId} is suspended and cannot log in.",
        presentationMessage: string.IsNullOrWhiteSpace(reason)
            ? "您的帳號已被停權，請聯絡客服。"
            : $"您的帳號已被停權：{reason}",
        errorCode: "MEMBER_SUSPENDED",
        statusCode: HttpStatusCode.Forbidden);
