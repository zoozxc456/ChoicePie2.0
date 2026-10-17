using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Events;

// 新增、修改或刪除題目都發這個事件。
public sealed record QuizQuestionsChangedDomainEvent(Guid QuizId) : BaseDomainEvent;
