using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Events;

// TagsChanged 讓訂閱者分辨是否影響以標籤組成的清單（例如標籤雲、相關題庫）。
public sealed record QuizDetailsUpdatedDomainEvent(Guid QuizId, bool TagsChanged) : BaseDomainEvent;
