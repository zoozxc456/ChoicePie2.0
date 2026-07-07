namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime LastModifiedAt { get; }
    DateTime? DeletedAt { get; }

    Guid? CreatorId { get; }
    Guid? LastModiferId { get; }
    Guid? DeleterId { get; }
}