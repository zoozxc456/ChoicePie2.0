using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;
using PasswordResetTokenAggregate = ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.PasswordResetToken;
using IPasswordResetTokenRepository = ChoicePie.Backend.Domain.Aggregates.PasswordResetToken.IPasswordResetTokenRepository;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetTokenRepository(ChoicePieDbContext context)
    : EfGenericRepository<PasswordResetTokenAggregate, ChoicePieDbContext>(context), IPasswordResetTokenRepository,
        IScopedDependency
{
    public override Task<PasswordResetTokenAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<PasswordResetTokenAggregate>().FirstOrDefaultAsync(t => t.Id == id, ct);
}
