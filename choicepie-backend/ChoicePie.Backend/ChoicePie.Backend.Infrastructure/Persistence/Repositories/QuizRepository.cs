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

    // 直接在資料庫做 share_count = share_count + 1，不經過追蹤中的聚合，同時分享不會互相覆蓋。
    public async Task<int?> IncrementShareCountAsync(Guid quizId, CancellationToken ct = default)
    {
        var updated = await context.Set<Quiz>()
            .Where(q => q.Id == quizId)
            .ExecuteUpdateAsync(s => s.SetProperty(q => q.ShareCount, q => q.ShareCount + 1), ct);

        if (updated == 0)
        {
            return null;
        }

        return await context.Set<Quiz>().Where(q => q.Id == quizId).Select(q => q.ShareCount).FirstAsync(ct);
    }

    // 與 ChallengeStats.RecordOutcome 同一個公式；SET 右邊讀到的都是更新前的值，所以兩個欄位在同一個 UPDATE 裡一致。
    public Task RecordChallengeOutcomeAsync(Guid quizId, bool passed, CancellationToken ct = default)
    {
        var outcome = passed ? 100m : 0m;
        return context.Set<Quiz>()
            .Where(q => q.Id == quizId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(q => q.Stats.PassRate,
                    q => (q.Stats.PassRate * q.Stats.Count + outcome) / (q.Stats.Count + 1))
                .SetProperty(q => q.Stats.Count, q => q.Stats.Count + 1), ct);
    }
}
