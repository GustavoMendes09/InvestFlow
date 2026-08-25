using FluentValidation;

namespace InvestFlow.Api.Features.Common;

public static class ValidatorExtensions
{
    public static async Task<IResult?> ValidateRequestAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        return validationResult.IsValid
            ? null
            : Results.ValidationProblem(validationResult.ToDictionary());
    }
}
