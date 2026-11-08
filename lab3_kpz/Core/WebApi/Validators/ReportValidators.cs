using FluentValidation;
using lab3_kpz.Core.WebApi.DTOs.Reports;

namespace lab3_kpz.Core.WebApi.Validators;

public sealed class CreateReportDtoValidator : AbstractValidator<CreateReportDto>
{
    public CreateReportDtoValidator()
    {
        RuleFor(x => x.ReportPeriodTo)
            .GreaterThanOrEqualTo(x => x.ReportPeriodFrom)
            .WithMessage("Report end date must be greater than or equal to start date.");

        RuleFor(x => x.ReportPeriodTo)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Report end date cannot be in the future.");
    }
}

