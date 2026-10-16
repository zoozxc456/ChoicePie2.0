using ChoicePie.Backend.Domain.Aggregates.Member.Events;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Specifications;
using ChoicePie.Backend.Shared.Application.Events;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.EventHandlers;

// 使用者關閉帳號後，其已發布的題庫不應繼續出現在公開列表：封存它們。
// EfUnitOfWork 在 commit 之後才派送事件，這裡是 at-least-once；規格只挑仍為 Published 的題庫，重送是冪等的。
public sealed class ArchivePublishedQuizzesOnAccountClosedHandler(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    : INotificationHandler<DomainEventNotification<MemberAccountClosedDomainEvent>>
{
    public async Task Handle(
        DomainEventNotification<MemberAccountClosedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var quizzes = await quizRepository.ListAsync(
            new PublishedQuizzesByCreatorSpecification(notification.DomainEvent.MemberId), cancellationToken);
        if (quizzes.Count == 0)
        {
            return;
        }

        foreach (var quiz in quizzes)
        {
            quiz.Archive();
            await quizRepository.UpdateAsync(quiz, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
