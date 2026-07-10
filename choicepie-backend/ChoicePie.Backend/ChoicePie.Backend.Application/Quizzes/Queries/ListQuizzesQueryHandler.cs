using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class ListQuizzesQueryHandler(IReadRepository readRepository)
    : IRequestHandler<ListQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(ListQuizzesQuery request, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<Quiz>().Where(q => q.IsPublic);

        if (!string.IsNullOrWhiteSpace(request.Tag))
        {
            query = query.Where(q => q.Tags.Contains(request.Tag));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(q => q.Title.Contains(request.Search));
        }

        var totalCount = query.Count();
        var pageItems = query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var creatorIds = pageItems.Select(q => q.OwnerId).Distinct().ToList();
        var creators = readRepository.Query<Member>()
            .Where(m => creatorIds.Contains(m.Id))
            .ToList()
            .ToDictionary(m => m.Id);

        var items = pageItems
            .Select(q =>
            {
                var creator = creators.GetValueOrDefault(q.OwnerId);
                return QuizSummaryDto.FromDomain(q, creator?.Name ?? "Unknown", creator?.Avatar);
            })
            .ToList();

        return Task.FromResult(new PagedResult<QuizSummaryDto>(items, request.PageNumber, request.PageSize, totalCount));
    }
}
