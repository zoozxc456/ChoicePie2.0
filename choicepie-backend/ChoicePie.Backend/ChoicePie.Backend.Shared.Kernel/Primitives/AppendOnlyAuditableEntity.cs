using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

public abstract class AppendOnlyAuditableEntity<TId> : Entity<TId>,
    IAppendOnlyAuditableEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid CreatorId { get; protected set; }

    protected void SetCreated(Guid creatorId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatorId = creatorId;
    }
}