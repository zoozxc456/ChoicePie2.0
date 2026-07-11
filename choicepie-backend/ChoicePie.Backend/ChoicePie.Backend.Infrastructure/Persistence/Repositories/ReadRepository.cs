using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;

namespace ChoicePie.Backend.Infrastructure.Persistence.Repositories;

public sealed class ReadRepository(ChoicePieDbContext context)
    : EfGenericReadRepository<ChoicePieDbContext>(context), IReadRepository, IScopedDependency;
