using ChoicePie.Backend.Domain.Aggregates.QuizReport;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class QuizReportRepository(ChoicePieDbContext context)
    : EfGenericRepository<QuizReport, ChoicePieDbContext>(context), IQuizReportRepository, IScopedDependency
{
    public override Task<QuizReport?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<QuizReport>().FirstOrDefaultAsync(r => r.Id == id, ct);
}
