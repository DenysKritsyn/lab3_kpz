using FluentValidation;
using lab3_kpz.Core.WebApi.DTOs.DayEvents;

namespace lab3_kpz.Core.WebApi.Validators;

public sealed class CreateDayEventDtoValidator : AbstractValidator<CreateDayEventDto>
{
    public CreateDayEventDtoValidator()
    {
        RuleFor(x => x.DiaryEntryId)
            .GreaterThan(0).WithMessage("DiaryEntryId must be greater than 0.");

        RuleFor(x => x.EventTypeId)
            .GreaterThan(0).WithMessage("EventTypeId must be greater than 0.");

        RuleFor(x => x.EventEndTime)
            .GreaterThanOrEqualTo(x => x.EventStartTime)
            .WithMessage("Event end time must be greater than or equal to start time.");

        RuleFor(x => x.EventNote)
            .MaximumLength(500).WithMessage("Event note must not exceed 500 characters.");
    }
}

public sealed class UpdateDayEventDtoValidator : AbstractValidator<UpdateDayEventDto>
{
    public UpdateDayEventDtoValidator()
    {
        RuleFor(x => x.EventTypeId)
            .GreaterThan(0).WithMessage("EventTypeId must be greater than 0.");

        RuleFor(x => x.EventEndTime)
            .GreaterThanOrEqualTo(x => x.EventStartTime)
            .WithMessage("Event end time must be greater than or equal to start time.");

        RuleFor(x => x.EventNote)
            .MaximumLength(500).WithMessage("Event note must not exceed 500 characters.");
    }
}

