using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Specifications;

public static class SpecificationEvaluator
{
    public static IQueryable<T> Apply<T>(IQueryable<T> queryable, ISpecification<T> specification)
        => queryable.Where(specification.ToExpression());
}
