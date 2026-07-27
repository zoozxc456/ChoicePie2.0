using ChoicePie.Backend.Domain.Aggregates.Comment;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class CommentRepository(ChoicePieDbContext context)
    : EfGenericRepository<Comment, ChoicePieDbContext>(context), ICommentRepository, IScopedDependency
{
    public override Task<Comment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Comment>().FirstOrDefaultAsync(c => c.Id == id, ct);
}
