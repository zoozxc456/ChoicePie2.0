using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.Caching.Quizzes;

/// <summary>以 tag 清除 <see cref="CachedQuizQueryService"/> 的快取；backplane 會把清除同步到其他節點。</summary>
public sealed class QuizCacheInvalidator(ICachingService cache, IReadRepository readRepository)
    : IQuizCacheInvalidator, IScopedDependency
{
    public async Task InvalidateQuizAsync(Guid quizId, CancellationToken cancellationToken) =>
        await cache.RemoveByTagAsync(QuizCacheKeys.QuizTag(quizId), cancellationToken);

    public async Task InvalidateCatalogAsync(CancellationToken cancellationToken) =>
        await cache.RemoveByTagAsync(QuizCacheKeys.CatalogTag, cancellationToken);

    // 寫入快取時還不知道創作者是誰（tag 必須在查詢前給定），所以無法掛 creator tag；
    // 改成查出該創作者的題庫逐一清除。改名、換頭像很少發生，多一次輕量查詢可以接受。
    public async Task InvalidateCreatorAsync(Guid creatorId, CancellationToken cancellationToken)
    {
        var quizIds = readRepository.Query<Quiz>()
            .Where(q => q.CreatorId == creatorId)
            .Select(q => q.Id)
            .ToList();

        foreach (var quizId in quizIds)
        {
            await InvalidateQuizAsync(quizId, cancellationToken);
        }
    }
}
