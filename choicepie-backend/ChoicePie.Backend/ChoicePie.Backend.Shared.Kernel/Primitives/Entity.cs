using System.ComponentModel.DataAnnotations;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

public abstract class Entity<TId>
{
    [Key] public TId Id { get; protected init; }

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

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        return a?.Equals(b) ?? b is null;
    }

    public static bool operator !=(Entity<TId>? a, Entity<TId>? b)
    {
        return !(a == b);
    }
}