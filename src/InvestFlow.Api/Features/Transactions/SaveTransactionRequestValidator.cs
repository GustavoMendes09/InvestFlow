using FluentValidation;

namespace InvestFlow.Api.Features.Transactions;

public sealed class SaveTransactionRequestValidator : AbstractValidator<SaveTransactionRequest>
{
    public SaveTransactionRequestValidator()
    {
        RuleFor(request => request.Amount).GreaterThan(0);
        RuleFor(request => request.Date).NotEmpty();
        RuleFor(request => request.Description).MaximumLength(160);
    }
}
