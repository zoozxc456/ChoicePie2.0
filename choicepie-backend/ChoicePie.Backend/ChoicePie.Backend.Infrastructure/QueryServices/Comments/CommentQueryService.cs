using ChoicePie.Backend.Application.Comments.Contracts;
using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.QueryServices.Comments;

public sealed class CommentQueryService(IReadRepository readRepository) : ICommentQueryService, IScopedDependency
{
    public Task<IReadOnlyList<CommentDto>> ListByQuizIdAsync(Guid quizId, CancellationToken cancellationToken)
    {
        var comments =
            (from c in readRepository.Query<Comment>()
                where c.QuizId == quizId
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
                    c.CreatedAt))
            .ToList();

        return Task.FromResult<IReadOnlyList<CommentDto>>(comments);
    }
}
