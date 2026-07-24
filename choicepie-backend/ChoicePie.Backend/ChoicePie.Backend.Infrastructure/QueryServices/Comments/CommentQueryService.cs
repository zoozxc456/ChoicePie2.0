using ChoicePie.Backend.Application.Comments.Contracts;
using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.Comments;

public sealed class CommentQueryService(IReadRepository readRepository) : ICommentQueryService, IScopedDependency
{
    public Task<PagedResult<CommentDto>> ListByQuizIdAsync(
        Guid quizId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = readRepository.Query<Comment>().Where(c => c.QuizId == quizId);

        var totalCount = query.Count();

        var joined =
            from c in query
            join m in readRepository.Query<Member>() on c.CreatorId!.Value equals m.Id into authorGroup
            from author in authorGroup.DefaultIfEmpty()
            orderby c.CreatedAt descending
            select new CommentDto(
                c.Id,
                c.QuizId,
                c.CreatorId!.Value,
                author != null ? author.Name : "Unknown",
                author != null ? author.Avatar : null,
                c.Text,
                c.CreatedAt);

        var items = joined
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(new PagedResult<CommentDto>(items, pageNumber, pageSize, totalCount));
    }
}
