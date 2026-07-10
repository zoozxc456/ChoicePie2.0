using System.Linq.Expressions;

namespace ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}
