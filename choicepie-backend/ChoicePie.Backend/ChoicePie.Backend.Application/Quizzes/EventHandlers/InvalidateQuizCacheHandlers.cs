using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Events;
using ChoicePie.Backend.Shared.Application.Events;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.EventHandlers;

// 題庫內容、狀態或創作者資料改變後清掉讀取快取。事件在 commit 之後才分派，
// 所以清完之後重新填入的一定是新資料。作答與分享次數不在這裡清，等 TTL 自然過期。
public sealed class InvalidateQuizCacheOnDetailsUpdatedHandler(IQuizCacheInvalidator invalidator)
    : INotificationHandler<DomainEventNotification<QuizDetailsUpdatedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<QuizDetailsUpdatedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await invalidator.InvalidateQuizAsync(notification.DomainEvent.QuizId, cancellationToken);

        if (notification.DomainEvent.TagsChanged)
        {
            await invalidator.InvalidateCatalogAsync(cancellationToken);
        }
    }
}

public sealed class InvalidateQuizCacheOnCoverUpdatedHandler(IQuizCacheInvalidator invalidator)
    : INotificationHandler<DomainEventNotification<QuizCoverUpdatedDomainEvent>>
{
    public Task Handle(DomainEventNotification<QuizCoverUpdatedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateQuizAsync(notification.DomainEvent.QuizId, cancellationToken);
}

public sealed class InvalidateQuizCacheOnQuestionsChangedHandler(IQuizCacheInvalidator invalidator)
    : INotificationHandler<DomainEventNotification<QuizQuestionsChangedDomainEvent>>
{
    public Task Handle(DomainEventNotification<QuizQuestionsChangedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateQuizAsync(notification.DomainEvent.QuizId, cancellationToken);
}

// 狀態改變會影響題庫是否出現在公開清單中，所以連跨題庫的資料一起清。
public sealed class InvalidateQuizCacheOnStatusChangedHandler(IQuizCacheInvalidator invalidator)
    : INotificationHandler<DomainEventNotification<QuizStatusChangedDomainEvent>>
{
    public async Task Handle(DomainEventNotification<QuizStatusChangedDomainEvent> notification,
        CancellationToken cancellationToken)
    {
        await invalidator.InvalidateQuizAsync(notification.DomainEvent.QuizId, cancellationToken);
        await invalidator.InvalidateCatalogAsync(cancellationToken);
    }
}

public sealed class InvalidateQuizCacheOnCreatorProfileChangedHandler(IQuizCacheInvalidator invalidator)
    : INotificationHandler<DomainEventNotification<MemberProfileChangedDomainEvent>>
{
    public Task Handle(DomainEventNotification<MemberProfileChangedDomainEvent> notification,
        CancellationToken cancellationToken) =>
        invalidator.InvalidateCreatorAsync(notification.DomainEvent.MemberId, cancellationToken);
}
