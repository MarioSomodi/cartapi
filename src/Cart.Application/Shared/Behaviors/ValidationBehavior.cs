using FluentValidation;
using Cart.Application.Shared;
using MediatR;

namespace Cart.Application.Shared.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        ValidationContext<TRequest> context = new(request);
        FluentValidation.Results.ValidationResult[] results =
            await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        FluentValidation.Results.ValidationFailure[] failures =
            results
                .SelectMany(result => result.Errors)
                .Where(failure => failure is not null)
                .ToArray();

        if (failures.Length == 0)
        {
            return await next(cancellationToken);
        }

        string description = string.Join(
            "; ",
            failures
                .Select(failure => failure.ErrorMessage)
                .Distinct(StringComparer.Ordinal));

        return CreateFailure(ApplicationErrors.Validation.Failed(description));
    }

    private static TResponse CreateFailure(Error error)
    {
        Type responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type genericType = responseType.GetGenericArguments()[0];
            Type closedType = typeof(Result<>).MakeGenericType(genericType);
            System.Reflection.MethodInfo? method = closedType.GetMethod(nameof(Result<object>.Failure), [typeof(Error)]);

            return (TResponse)method!.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior only supports Result responses. {responseType.Name} is not supported.");
    }
}
