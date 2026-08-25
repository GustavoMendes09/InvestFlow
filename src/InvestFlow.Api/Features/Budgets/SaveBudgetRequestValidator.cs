using FluentValidation;

namespace InvestFlow.Api.Features.Budgets;

public sealed class SaveBudgetRequestValidator : AbstractValidator<SaveBudgetRequest>
{
    public SaveBudgetRequestValidator()
    {
        RuleFor(request => request.Month).NotEmpty();
        RuleFor(request => request.Amount).GreaterThanOrEqualTo(0);
    }
}
