using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

/// <summary>
/// Walks a specification's criteria expression and throws if it references a [NotMapped] member,
/// since EF Core cannot translate those to SQL - they only work against an already-loaded, in-memory
/// entity, not a queryable that gets pushed down to the database.
/// </summary>
internal sealed class NotMappedMemberVisitor : ExpressionVisitor
{
    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Member.GetCustomAttribute<NotMappedAttribute>() is not null)
        {
            throw new InvalidOperationException(
                $"Specification criteria references '{node.Member.DeclaringType?.Name}.{node.Member.Name}', " +
                "which is marked [NotMapped]. EF Core cannot translate this to SQL - filter on the " +
                "underlying mapped property instead.");
        }

        return base.VisitMember(node);
    }
}
