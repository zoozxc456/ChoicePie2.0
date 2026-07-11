namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

public interface IAppendOnlyAuditableEntity
{
    DateTime CreatedAt { get; }
    Guid CreatorId { get; }
}