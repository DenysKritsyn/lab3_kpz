using FluentValidation;
using lab3_kpz.Core.WebApi.DTOs.DiaryEntries;
using lab3_kpz.Core.WebApi.Validators;

namespace lab3_kpz.Core.WebApi.Validators;

public sealed class CreateIntervalSymptomDtoValidator : AbstractValidator<CreateIntervalSymptomDto>
{
    public CreateIntervalSymptomDtoValidator()
    {
        RuleFor(x => x.SymptomId)
            .GreaterThan(0).WithMessage("SymptomId must be greater than 0.");

        RuleFor(x => x.SeverityLevelId)
            .InclusiveBetween(1, 4).WithMessage("SeverityLevelId must be between 1 and 4.");

        RuleFor(x => x.SymptomNote)
            .MaximumLength(500).WithMessage("Symptom note must not exceed 500 characters.");
    }
}

public sealed class CreateDayIntervalDtoValidator : AbstractValidator<CreateDayIntervalDto>
{
    public CreateDayIntervalDtoValidator()
    {
        RuleFor(x => x.IntervalEndTime)
            .GreaterThanOrEqualTo(x => x.IntervalStartTime)
            .WithMessage("Interval end time must be greater than or equal to start time.");

        RuleFor(x => x.ConditionStateId)
            .InclusiveBetween(1, 5).WithMessage("ConditionStateId must be between 1 and 5.");

        RuleFor(x => x.IntervalNote)
            .MaximumLength(500).WithMessage("Interval note must not exceed 500 characters.");

        RuleForEach(x => x.Symptoms)
            .SetValidator(new CreateIntervalSymptomDtoValidator());
    }
}

public sealed class CreateDiaryEntryDtoValidator : AbstractValidator<CreateDiaryEntryDto>
{
    public CreateDiaryEntryDtoValidator()
    {
        RuleFor(x => x.EntryDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Diary entry date cannot be in the future.");

        RuleFor(x => x.EntryNote)
            .MaximumLength(1000).WithMessage("Entry note must not exceed 1000 characters.");

        RuleForEach(x => x.DayIntervals)
            .SetValidator(new CreateDayIntervalDtoValidator());

        RuleForEach(x => x.DayEvents)
            .SetValidator(new CreateDayEventDtoValidator());

        RuleForEach(x => x.Measurements)
            .SetValidator(new CreateMeasurementDtoValidator());
    }
}

public sealed class UpdateDiaryEntryDtoValidator : AbstractValidator<UpdateDiaryEntryDto>
{
    public UpdateDiaryEntryDtoValidator()
    {
        RuleFor(x => x.EntryNote)
            .MaximumLength(1000).WithMessage("Entry note must not exceed 1000 characters.");

        RuleForEach(x => x.DayIntervals)
            .SetValidator(new CreateDayIntervalDtoValidator());

        RuleForEach(x => x.DayEvents)
            .SetValidator(new CreateDayEventDtoValidator());

        RuleForEach(x => x.Measurements)
            .SetValidator(new CreateMeasurementDtoValidator());
    }
}

public sealed class DiaryEntryQueryDtoValidator : AbstractValidator<DiaryEntryQueryDto>
{
    public DiaryEntryQueryDtoValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(x => x.FromDate!.Value)
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
            .WithMessage("ToDate must be greater than or equal to FromDate.");
    }
}

