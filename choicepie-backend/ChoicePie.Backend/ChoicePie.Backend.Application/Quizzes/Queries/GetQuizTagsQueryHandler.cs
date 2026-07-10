using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class GetQuizTagsQueryHandler(IReadRepository readRepository)
    : IRequestHandler<GetQuizTagsQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetQuizTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = readRepository.Query<Quiz>()
            .Where(q => q.IsPublic)
            .SelectMany(q => q.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(tags);
    }
}
