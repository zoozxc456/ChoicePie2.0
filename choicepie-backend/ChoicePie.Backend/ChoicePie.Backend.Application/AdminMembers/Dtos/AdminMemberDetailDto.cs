namespace ChoicePie.Backend.Application.AdminMembers.Dtos;

public sealed record AdminMemberDetailDto(
    Guid Id,
    string Name,
    string Email,
    string? Avatar,
    bool IsSuspended,
    string? SuspendedReason,
    DateTime? SuspendedUntil,
    DateTime? LastAiGenerationAt,
    Guid? TierId,
    string? TierName,
    DateTime CreatedAt);
