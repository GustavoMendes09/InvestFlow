using FluentValidation;

namespace InvestFlow.Api.Features.Categories;

public sealed class SaveCategoryRequestValidator : AbstractValidator<SaveCategoryRequest>
{
    public SaveCategoryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(60);
        RuleFor(request => request.Color).Matches("^#[0-9a-fA-F]{6}$");
    }
}
