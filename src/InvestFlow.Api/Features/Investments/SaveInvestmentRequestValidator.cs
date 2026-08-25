using FluentValidation;

namespace InvestFlow.Api.Features.Investments;

public sealed class SaveInvestmentRequestValidator : AbstractValidator<SaveInvestmentRequest>
{
    public SaveInvestmentRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.AssetClass).NotEmpty().MaximumLength(60);
        RuleFor(request => request.InvestedAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.CurrentValue).GreaterThanOrEqualTo(0);
        RuleFor(request => request.UpdatedAt).NotEmpty();
    }
}
