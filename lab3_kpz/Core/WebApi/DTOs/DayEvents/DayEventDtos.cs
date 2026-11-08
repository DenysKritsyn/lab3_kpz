using lab3_kpz.Core.WebApi.DTOs.Common;

namespace lab3_kpz.Core.WebApi.DTOs.DayEvents;

public sealed record DayEventResponseDto(
    int DayEventId,
    int DiaryEntryId,
    int EventTypeId,
    string? EventTypeName,
    TimeOnly EventStartTime,
    TimeOnly EventEndTime,
    bool IsPreciseTime,
    string? EventNote);

public sealed record CreateDayEventDto(
    int DiaryEntryId,
    int EventTypeId,
    TimeOnly EventStartTime,
    TimeOnly EventEndTime,
    bool IsPreciseTime,
    string? EventNote);

public sealed record UpdateDayEventDto(
    int EventTypeId,
    TimeOnly EventStartTime,
    TimeOnly EventEndTime,
    bool IsPreciseTime,
    string? EventNote);

public sealed class DayEventQueryDto : PaginationQueryDto
{
    public int? DiaryEntryId { get; set; }
    public int? EventTypeId { get; set; }
}