using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetRelatedQuizzesQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<GetRelatedQuizzesQuery, IReadOnlyList<QuizSummaryDto>>
{
    public Task<IReadOnlyList<QuizSummaryDto>> Handle(GetRelatedQuizzesQuery request, CancellationToken cancellationToken) =>
        quizQueryService.GetRelatedAsync(request.QuizId, request.Limit, cancellationToken);
}
