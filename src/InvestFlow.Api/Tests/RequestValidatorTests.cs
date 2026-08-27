using InvestFlow.Api.Domain.Goals;
using InvestFlow.Api.Domain.Transactions;
using InvestFlow.Api.Features.Accounts;
using InvestFlow.Api.Features.Budgets;
using InvestFlow.Api.Features.Categories;
using InvestFlow.Api.Features.Goals;
using InvestFlow.Api.Features.Investments;
using InvestFlow.Api.Features.Transactions;

namespace InvestFlow.Api.Tests;

public sealed class RequestValidatorTests
{
    [Fact]
    public void AccountValidator_RejectsBlankNameAndNegativeBalance()
    {
        var result = new SaveAccountRequestValidator().Validate(new SaveAccountRequest("", -1, false));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveAccountRequest.Name));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveAccountRequest.Balance));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("#12345")]
    [InlineData("#12345G")]
    [InlineData("")]
    public void CategoryValidator_RejectsInvalidHexColours(string colour)
    {
        var result = new SaveCategoryRequestValidator().Validate(
            new SaveCategoryRequest("Groceries", colour, false));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveCategoryRequest.Color));
    }

    [Fact]
    public void CategoryValidator_AcceptsSixDigitHexColour()
    {
        var result = new SaveCategoryRequestValidator().Validate(
            new SaveCategoryRequest("Groceries", "#16a34a", false));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void TransactionValidator_RejectsNonPositiveAmountAndMissingDate()
    {
        var result = new SaveTransactionRequestValidator().Validate(
            new SaveTransactionRequest(TransactionType.Expense, 0, default, null, null, null));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveTransactionRequest.Amount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveTransactionRequest.Date));
    }

    [Fact]
    public void BudgetValidator_RejectsNegativeAmountAndMissingMonth()
    {
        var result = new SaveBudgetRequestValidator().Validate(new SaveBudgetRequest(default, -1));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveBudgetRequest.Month));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveBudgetRequest.Amount));
    }

    [Fact]
    public void InvestmentValidator_RejectsMissingFieldsAndNegativeValues()
    {
        var result = new SaveInvestmentRequestValidator().Validate(
            new SaveInvestmentRequest("", "", -1, -1, default));

        Assert.False(result.IsValid);
        Assert.Equal(5, result.Errors.Select(error => error.PropertyName).Distinct().Count());
    }

    [Fact]
    public void ContributionValidator_RequiresPositiveAmountAndDate()
    {
        var result = new RecordContributionRequestValidator().Validate(
            new RecordContributionRequest(0, default));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RecordContributionRequest.Amount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RecordContributionRequest.Date));
    }

    [Fact]
    public void GoalValidator_RequiresTargetAndNonNegativeCurrentAmount()
    {
        var result = new SaveGoalRequestValidator().Validate(
            new SaveGoalRequest("Emergency fund", GoalType.EmergencyFund, 0, -1, null));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveGoalRequest.TargetAmount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(SaveGoalRequest.CurrentAmount));
    }

    [Fact]
    public void ValidRequests_PassTheirValidators()
    {
        var today = new DateOnly(2026, 8, 26);

        Assert.True(new SaveAccountRequestValidator().Validate(
            new SaveAccountRequest("Everyday", 100, false)).IsValid);
        Assert.True(new SaveBudgetRequestValidator().Validate(
            new SaveBudgetRequest(today, 500)).IsValid);
        Assert.True(new SaveTransactionRequestValidator().Validate(
            new SaveTransactionRequest(TransactionType.Income, 1_000, today, null, "Salary", null)).IsValid);
        Assert.True(new SaveInvestmentRequestValidator().Validate(
            new SaveInvestmentRequest("Index fund", "ETF", 1_000, 1_100, today)).IsValid);
        Assert.True(new RecordContributionRequestValidator().Validate(
            new RecordContributionRequest(100, today)).IsValid);
        Assert.True(new SaveGoalRequestValidator().Validate(
            new SaveGoalRequest("Travel", GoalType.Travel, 5_000, 500, today.AddYears(1))).IsValid);
    }
}
