namespace ChoicePie.Backend.Application.MembershipTiers.Dtos;

public sealed record MembershipTierDto(
    Guid Id,
    string Name,
    int DailyGenerationLimit,
    int DailyTokenBudget,
    bool IsDefault,
    DateTime CreatedAt);
