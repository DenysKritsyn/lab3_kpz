using FluentValidation;
using lab3_kpz.Core.WebApi.DTOs.Symptoms;

namespace lab3_kpz.Core.WebApi.Validators;

public sealed class CreateSymptomDtoValidator : AbstractValidator<CreateSymptomDto>
{
    public CreateSymptomDtoValidator()
    {
        RuleFor(x => x.SymptomName)
            .NotEmpty().WithMessage("Symptom name is required.")
            .MaximumLength(100).WithMessage("Symptom name must not exceed 100 characters.");
    }
}

public sealed class UpdateSymptomDtoValidator : AbstractValidator<UpdateSymptomDto>
{
    public UpdateSymptomDtoValidator()
    {
        RuleFor(x => x.SymptomName)
            .NotEmpty().WithMessage("Symptom name is required.")
            .MaximumLength(100).WithMessage("Symptom name must not exceed 100 characters.");
    }
}

public sealed class SymptomQueryDtoValidator : AbstractValidator<SymptomQueryDto>
{
    public SymptomQueryDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}

