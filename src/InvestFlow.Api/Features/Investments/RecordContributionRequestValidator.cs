using FluentValidation;

namespace InvestFlow.Api.Features.Investments;

public sealed class RecordContributionRequestValidator : AbstractValidator<RecordContributionRequest>
{
    public RecordContributionRequestValidator()
    {
        RuleFor(request => request.Amount).GreaterThan(0);
        RuleFor(request => request.Date).NotEmpty();
    }
}
