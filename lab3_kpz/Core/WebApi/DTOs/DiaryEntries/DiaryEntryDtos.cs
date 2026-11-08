using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DayEvents;
using lab3_kpz.Core.WebApi.DTOs.Measurements;

namespace lab3_kpz.Core.WebApi.DTOs.DiaryEntries;

public sealed record IntervalSymptomResponseDto(
    int DayIntervalId,
    int SymptomId,
    string? SymptomName,
    int SeverityLevelId,
    string? SeverityName,
    string? SymptomNote);

public sealed record CreateIntervalSymptomDto(
    int SymptomId,
    int SeverityLevelId,
    string? SymptomNote);

public sealed record DayIntervalResponseDto(
    int DayIntervalId,
    int DiaryEntryId,
    TimeOnly IntervalStartTime,
    TimeOnly IntervalEndTime,
    int ConditionStateId,
    string? ConditionStateName,
    string? IntervalNote,
    IReadOnlyList<IntervalSymptomResponseDto> Symptoms);

public sealed record CreateDayIntervalDto(
    TimeOnly IntervalStartTime,
    TimeOnly IntervalEndTime,
    int ConditionStateId,
    string? IntervalNote,
    List<CreateIntervalSymptomDto>? Symptoms);

public sealed record DiaryEntryResponseDto(
    int DiaryEntryId,
    int UserId,
    DateOnly EntryDate,
    string? EntryNote,
    DateTime EntryCreatedAt,
    DateTime EntryUpdatedAt,
    IReadOnlyList<DayIntervalResponseDto> DayIntervals,
    IReadOnlyList<DayEventResponseDto> DayEvents,
    IReadOnlyList<MeasurementResponseDto> Measurements);

public sealed record DiaryEntrySummaryDto(
    int DiaryEntryId,
    int UserId,
    DateOnly EntryDate,
    string? EntryNote,
    int IntervalsCount,
    int EventsCount,
    int MeasurementsCount,
    DateTime EntryCreatedAt,
    DateTime EntryUpdatedAt);

public sealed record CreateDiaryEntryDto(
    DateOnly EntryDate,
    string? EntryNote,
    List<CreateDayIntervalDto>? DayIntervals,
    List<CreateDayEventDto>? DayEvents,
    List<CreateMeasurementDto>? Measurements);

public sealed record UpdateDiaryEntryDto(
    string? EntryNote,
    List<CreateDayIntervalDto>? DayIntervals,
    List<CreateDayEventDto>? DayEvents,
    List<CreateMeasurementDto>? Measurements);

public sealed class DiaryEntryQueryDto : PaginationQueryDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? SymptomId { get; set; }
    public string? SearchNote { get; set; }
}