using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class MemberRepository(ChoicePieDbContext context)
    : EfGenericRepository<Member, ChoicePieDbContext>(context), IMemberRepository, IScopedDependency
{
    public override Task<Member?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<Member>().FirstOrDefaultAsync(m => m.Id == id, ct);
}
