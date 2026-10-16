using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.AiUsageLog;

public sealed class AiUsageLog : AggregateRoot<Guid>
{
    public Guid MemberId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int TokensUsed { get; private set; }

    private AiUsageLog()
    {
    }

    public static AiUsageLog Create(Guid memberId, string provider, string model, int tokensUsed)
    {
        var log = new AiUsageLog
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Provider = provider,
            Model = model,
            TokensUsed = tokensUsed
        };

        log.SetCreated(memberId);

        return log;
    }
}
