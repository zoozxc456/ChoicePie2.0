using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Shared.Application.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Shared.Application.Behaviors;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext(request);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            throw new AggregateValidationException(GroupByMember(results));
        }

        return next(cancellationToken);
    }

    private static Dictionary<string, string[]> GroupByMember(List<ValidationResult> results)
    {
        return results
            .SelectMany(result =>
            {
                var memberNames = result.MemberNames.Any() ? result.MemberNames : [string.Empty];
                return memberNames.Select(memberName => (memberName, message: result.ErrorMessage ?? "驗證失敗"));
            })
            .GroupBy(x => x.memberName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.message).ToArray());
    }
}
