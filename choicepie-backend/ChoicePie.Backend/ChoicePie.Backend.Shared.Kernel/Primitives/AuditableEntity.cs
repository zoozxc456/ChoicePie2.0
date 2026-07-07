using System.ComponentModel.DataAnnotations.Schema;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

public abstract class AuditableEntity<TId> : Entity<TId>,
    IAuditableEntity
{
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? CreatorId { get; protected set; }
    public DateTime LastModifiedAt { get; protected set; } = DateTime.UtcNow;
    public Guid? LastModiferId { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeleterId { get; private set; }

    [NotMapped] public bool IsDeleted => !DeletedAt.HasValue;

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        return Id?.Equals(other.Id) ?? false;
    }

    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }

    public static bool operator ==(AuditableEntity<TId>? a, AuditableEntity<TId>? b)
    {
        return a?.Equals(b) ?? b is null;
    }

    public static bool operator !=(AuditableEntity<TId>? a, AuditableEntity<TId>? b)
    {
        return !(a == b);
    }

    protected void SetCreated(Guid userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatorId = userId;

        LastModifiedAt = CreatedAt;
        LastModiferId = userId;
    }

    public void Touch()
    {
        LastModifiedAt = DateTime.UtcNow;
    }

    public void Touch(Guid userId)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModiferId = userId;
    }

    public void Delete(Guid userId)
    {
        DeletedAt = DateTime.UtcNow;
        DeleterId = userId;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
    }
}