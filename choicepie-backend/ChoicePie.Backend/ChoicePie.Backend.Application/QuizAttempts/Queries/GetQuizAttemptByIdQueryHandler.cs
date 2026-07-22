using ChoicePie.Backend.Application.QuizAttempts.Contracts;
using ChoicePie.Backend.Application.QuizAttempts.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.QuizAttempts.Queries;

public sealed class GetQuizAttemptByIdQueryHandler(
    IQuizAttemptQueryService quizAttemptQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetQuizAttemptByIdQuery, QuizAttemptResultDto>
{
    public async Task<QuizAttemptResultDto> Handle(GetQuizAttemptByIdQuery request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();
        var result = await quizAttemptQueryService.GetByIdAsync(request.Id, cancellationToken)
                     ?? throw new QuizAttemptNotFoundException(request.Id);

        if (result.MemberId != memberId)
        {
            throw new QuizAttemptForbiddenException(result.Id, memberId);
        }

        return result;
    }
}
