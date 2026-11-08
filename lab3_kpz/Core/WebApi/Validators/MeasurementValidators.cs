using FluentValidation;
using lab3_kpz.Core.WebApi.DTOs.Measurements;

namespace lab3_kpz.Core.WebApi.Validators;

public sealed class CreateMetricDtoValidator : AbstractValidator<CreateMetricDto>
{
    public CreateMetricDtoValidator()
    {
        RuleFor(x => x.MetricUnitId)
            .GreaterThan(0).WithMessage("MetricUnitId must be greater than 0.");

        RuleFor(x => x.MetricName)
            .NotEmpty().WithMessage("Metric name is required.")
            .MaximumLength(100).WithMessage("Metric name must not exceed 100 characters.");

        RuleFor(x => x.MetricValue)
            .GreaterThan(0).WithMessage("Metric value must be greater than 0.");
    }
}

public sealed class CreateMeasurementDtoValidator : AbstractValidator<CreateMeasurementDto>
{
    public CreateMeasurementDtoValidator()
    {
        RuleFor(x => x.DiaryEntryId)
            .GreaterThan(0).WithMessage("DiaryEntryId must be greater than 0.");

        RuleFor(x => x.Metrics)
            .NotEmpty().WithMessage("At least one metric is required for a measurement.");

        RuleForEach(x => x.Metrics)
            .SetValidator(new CreateMetricDtoValidator());
    }
}

