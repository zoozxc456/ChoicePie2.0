namespace ChoicePie.Backend.Application.AdminMembers.Dtos;

public sealed record AdminMemberSummaryDto(
    Guid Id,
    string Name,
    string Email,
    bool IsSuspended,
    string? SuspendedReason,
    DateTime? SuspendedUntil,
    DateTime CreatedAt);
