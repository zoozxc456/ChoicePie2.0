using ChoicePie.Backend.Application.QuizAttempts.Contracts;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Queries;

public sealed class ListQuizAttemptHistoryQueryHandler(
    IQuizAttemptQueryService quizAttemptQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListQuizAttemptHistoryQuery, IReadOnlyList<QuizAttemptHistoryItemDto>>
{
    public Task<IReadOnlyList<QuizAttemptHistoryItemDto>> Handle(
        ListQuizAttemptHistoryQuery request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return quizAttemptQueryService.ListHistoryAsync(request.QuizId, memberId, cancellationToken);
    }
}
