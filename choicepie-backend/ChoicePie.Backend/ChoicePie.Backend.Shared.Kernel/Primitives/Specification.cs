using System.Linq.Expressions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

public abstract class Specification<T>(Expression<Func<T, bool>> criteria) : ISpecification<T>
{
    public Expression<Func<T, bool>> ToExpression() => criteria;
}
