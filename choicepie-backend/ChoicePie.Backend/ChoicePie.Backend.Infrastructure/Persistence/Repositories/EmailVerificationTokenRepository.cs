using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.EntityFrameworkCore;
using EmailVerificationTokenAggregate = ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.EmailVerificationToken;
using IEmailVerificationTokenRepository = ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken.IEmailVerificationTokenRepository;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationTokenRepository(ChoicePieDbContext context)
    : EfGenericRepository<EmailVerificationTokenAggregate, ChoicePieDbContext>(context), IEmailVerificationTokenRepository,
        IScopedDependency
{
    public override Task<EmailVerificationTokenAggregate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<EmailVerificationTokenAggregate>().FirstOrDefaultAsync(t => t.Id == id, ct);
}
