using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.Events;

// 發布、取消發布、封存、取消封存、刪除、下架、恢復下架都會改變狀態，統一由這個事件通知。
public sealed record QuizStatusChangedDomainEvent(Guid QuizId, QuizStatus PreviousStatus, QuizStatus CurrentStatus)
    : BaseDomainEvent;
