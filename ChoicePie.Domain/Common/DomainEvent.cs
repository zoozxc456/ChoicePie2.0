namespace ChoicePie.Domain.Common;

public abstract class DomainEvent
{
    public DateTime OccuredAt => DateTime.UtcNow;
}