using FluentValidation;

namespace InvestFlow.Api.Features.Accounts;

public sealed class SaveAccountRequestValidator : AbstractValidator<SaveAccountRequest>
{
    public SaveAccountRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Balance).GreaterThanOrEqualTo(0);
    }
}
