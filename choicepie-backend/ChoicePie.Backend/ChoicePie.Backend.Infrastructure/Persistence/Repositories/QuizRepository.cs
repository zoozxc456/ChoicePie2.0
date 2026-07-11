using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class QuizRepository(ChoicePieDbContext context)
    : EfGenericRepository<Quiz, ChoicePieDbContext>(context), IQuizRepository, IScopedDependency
{
    protected override IQueryable<Quiz> ApplyInclude(IQueryable<Quiz> queryable) =>
        queryable.Include(q => q.Questions);

    public override Task<Quiz?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ApplyInclude(context.Set<Quiz>()).FirstOrDefaultAsync(q => q.Id == id, ct);
}
