using System.Linq.Expressions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

public abstract class Specification<T> : ISpecification<T>
{
    private readonly Expression<Func<T, bool>> _criteria;

    protected Specification(Expression<Func<T, bool>> criteria)
    {
        new NotMappedMemberVisitor().Visit(criteria);
        _criteria = criteria;
    }

    public Expression<Func<T, bool>> ToExpression() => _criteria;
}
