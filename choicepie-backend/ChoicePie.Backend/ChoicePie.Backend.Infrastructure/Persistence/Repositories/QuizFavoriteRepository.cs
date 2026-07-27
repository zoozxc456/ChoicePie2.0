using ChoicePie.Backend.Domain.Aggregates.QuizFavorite;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class QuizFavoriteRepository(ChoicePieDbContext context)
    : EfGenericRepository<QuizFavorite, ChoicePieDbContext>(context), IQuizFavoriteRepository, IScopedDependency
{
    public override Task<QuizFavorite?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<QuizFavorite>().FirstOrDefaultAsync(f => f.Id == id, ct);
}
