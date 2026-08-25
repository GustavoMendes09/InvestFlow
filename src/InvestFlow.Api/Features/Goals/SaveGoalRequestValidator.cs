using FluentValidation;

namespace InvestFlow.Api.Features.Goals;

public sealed class SaveGoalRequestValidator : AbstractValidator<SaveGoalRequest>
{
    public SaveGoalRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.TargetAmount).GreaterThan(0);
        RuleFor(request => request.CurrentAmount).GreaterThanOrEqualTo(0);
    }
}
