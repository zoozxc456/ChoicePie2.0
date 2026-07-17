using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.Primitives;
using Microsoft.EntityFrameworkCore;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

public sealed class TestWidgetRepository(TestDbContext context) : EfGenericRepository<TestWidget, TestDbContext>(context)
{
    public override Task<TestWidget?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<TestWidget>().FirstOrDefaultAsync(w => w.Id == id, ct);
}

public sealed class TestWidgetNameStartsWithSpecification(string prefix) : Specification<TestWidget>(w => w.Name.StartsWith(prefix));
