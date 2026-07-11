using ChoicePie.Backend.Application.Quizzes.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizTagsQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<GetQuizTagsQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetQuizTagsQuery request, CancellationToken cancellationToken) =>
        quizQueryService.GetTagsAsync(cancellationToken);
}
