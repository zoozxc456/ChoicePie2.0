using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class QuizAttemptRepository(ChoicePieDbContext context)
    : EfGenericRepository<QuizAttemptAggregate, ChoicePieDbContext>(context), IQuizAttemptRepository,
        IScopedDependency
{
    protected override IQueryable<QuizAttemptAggregate> ApplyInclude(IQueryable<QuizAttemptAggregate> queryable) =>
        queryable.Include(a => a.Answers);

    public override Task<QuizAttemptAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        ApplyInclude(context.Set<QuizAttemptAggregate>()).FirstOrDefaultAsync(a => a.Id == id, ct);
}
