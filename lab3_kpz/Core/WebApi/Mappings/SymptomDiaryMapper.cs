using lab3_kpz.Core.WebApi.DTOs.DayEvents;
using lab3_kpz.Core.WebApi.DTOs.DiaryEntries;
using lab3_kpz.Core.WebApi.DTOs.Measurements;
using lab3_kpz.Core.WebApi.DTOs.Reports;
using lab3_kpz.Core.WebApi.DTOs.Symptoms;
using lab3_kpz.Entities;
using Riok.Mapperly.Abstractions;

namespace lab3_kpz.Core.WebApi.Mappings;

[Mapper]
public partial class SymptomDiaryMapper
{
    // Symptom
    public partial SymptomResponseDto ToDto(Symptom entity);
    public partial IReadOnlyList<SymptomResponseDto> ToDto(IEnumerable<Symptom> entities);

    [MapperIgnoreTarget(nameof(Symptom.SymptomId))]
    [MapperIgnoreTarget(nameof(Symptom.IntervalSymptoms))]
    public partial Symptom ToEntity(CreateSymptomDto dto);

    // DayEvent
    [MapProperty("EventType.EventTypeName", nameof(DayEventResponseDto.EventTypeName))]
    public partial DayEventResponseDto ToDto(DayEvent entity);
    public partial IReadOnlyList<DayEventResponseDto> ToDto(IEnumerable<DayEvent> entities);

    [MapperIgnoreTarget(nameof(DayEvent.DayEventId))]
    [MapperIgnoreTarget(nameof(DayEvent.DiaryEntry))]
    [MapperIgnoreTarget(nameof(DayEvent.EventType))]
    public partial DayEvent ToEntity(CreateDayEventDto dto);

    // Metric
    [MapProperty("MetricUnit.UnitName", nameof(MetricResponseDto.UnitName))]
    public partial MetricResponseDto ToDto(Metric entity);

    [MapperIgnoreTarget(nameof(Metric.MetricId))]
    [MapperIgnoreTarget(nameof(Metric.MeasurementId))]
    [MapperIgnoreTarget(nameof(Metric.Measurement))]
    [MapperIgnoreTarget(nameof(Metric.MetricUnit))]
    public partial Metric ToEntity(CreateMetricDto dto);

    // Measurement
    public partial MeasurementResponseDto ToDto(Measurement entity);
    public partial IReadOnlyList<MeasurementResponseDto> ToDto(IEnumerable<Measurement> entities);

    [MapperIgnoreTarget(nameof(Measurement.MeasurementId))]
    [MapperIgnoreTarget(nameof(Measurement.DiaryEntry))]
    public partial Measurement ToEntity(CreateMeasurementDto dto);

    // Report
    public partial ReportResponseDto ToDto(Report entity);
    public partial IReadOnlyList<ReportResponseDto> ToDto(IEnumerable<Report> entities);

    // IntervalSymptom
    [MapProperty("Symptom.SymptomName", nameof(IntervalSymptomResponseDto.SymptomName))]
    [MapProperty("SeverityLevel.SeverityName", nameof(IntervalSymptomResponseDto.SeverityName))]
    public partial IntervalSymptomResponseDto ToDto(IntervalSymptom entity);

    // DayInterval
    [MapProperty("ConditionState.ConditionStateName", nameof(DayIntervalResponseDto.ConditionStateName))]
    [MapProperty(nameof(DayInterval.IntervalSymptoms), nameof(DayIntervalResponseDto.Symptoms))]
    public partial DayIntervalResponseDto ToDto(DayInterval entity);

    // DiaryEntry
    public partial DiaryEntryResponseDto ToDto(DiaryEntry entity);
    public partial IReadOnlyList<DiaryEntryResponseDto> ToDto(IEnumerable<DiaryEntry> entities);

    public DiaryEntrySummaryDto ToSummaryDto(DiaryEntry entity) =>
        new(
            entity.DiaryEntryId,
            entity.UserId,
            entity.EntryDate,
            entity.EntryNote,
            entity.DayIntervals?.Count ?? 0,
            entity.DayEvents?.Count ?? 0,
            entity.Measurements?.Count ?? 0,
            entity.EntryCreatedAt,
            entity.EntryUpdatedAt);

    public IReadOnlyList<DiaryEntrySummaryDto> ToSummaryDtoList(IEnumerable<DiaryEntry> entities) =>
        entities.Select(ToSummaryDto).ToList();
}
