using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.MembershipTier;

public sealed class MembershipTier : AggregateRoot<Guid>
{
    private const int MaxNameLength = 50;

    public string Name { get; private set; } = null!;
    public int DailyGenerationLimit { get; private set; }
    public int DailyTokenBudget { get; private set; }
    public bool IsDefault { get; private set; }

    private MembershipTier()
    {
    }

    public static MembershipTier Create(string name, int dailyGenerationLimit, int dailyTokenBudget, bool isDefault = false)
    {
        var tier = new MembershipTier { Id = Guid.NewGuid() };
        tier.SetCreated(tier.Id);
        tier.UpdateName(name);
        tier.UpdateLimits(dailyGenerationLimit, dailyTokenBudget);
        tier.IsDefault = isDefault;

        return tier;
    }

    public void Update(string name, int dailyGenerationLimit, int dailyTokenBudget)
    {
        UpdateName(name);
        UpdateLimits(dailyGenerationLimit, dailyTokenBudget);
        Touch();
    }

    private void UpdateName(string name)
    {
        var trimmed = name?.Trim();

        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length > MaxNameLength)
        {
            throw new InvalidMembershipTierException($"會員等級名稱不能為空，且長度不可超過 {MaxNameLength} 字。");
        }

        Name = trimmed;
    }

    private void UpdateLimits(int dailyGenerationLimit, int dailyTokenBudget)
    {
        if (dailyGenerationLimit < 0)
        {
            throw new InvalidMembershipTierException("每日生成次數上限不可為負數。");
        }

        if (dailyTokenBudget < 0)
        {
            throw new InvalidMembershipTierException("每日 token 上限不可為負數。");
        }

        DailyGenerationLimit = dailyGenerationLimit;
        DailyTokenBudget = dailyTokenBudget;
    }
}
