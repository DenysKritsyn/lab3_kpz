namespace Kpz.Lab2.Dtos;

public sealed record DiaryEntryDto(
    int DiaryEntryId,
    int UserId,
    string UserName,
    DateOnly EntryDate,
    string? EntryNote,
    DateTime EntryCreatedAt,
    DateTime EntryUpdatedAt);

public sealed record MeasurementDto(
    int MeasurementId,
    int DiaryEntryId,
    int MetricTypeId,
    string MetricName,
    string MetricUnit,
    decimal MeasurementValue,
    byte? MeasurementHour,
    string? MeasurementNote);

public sealed record SymptomOccurrenceDto(
    int DiaryEntryId,
    DateOnly EntryDate,
    string UserName,
    byte IntervalStartHour,
    byte IntervalEndHour,
    string SymptomName,
    string? SymptomCategory,
    string SeverityName,
    byte SeverityRank,
    string? SymptomNote);

public sealed record SymptomStatsDto(
    int SymptomId,
    string SymptomName,
    string? SymptomCategory,
    int OccurrencesCount,
    int TotalHoursDuration,
    double AverageSeverityRank,
    byte MaxSeverityRank);

public sealed record MetricStatsDto(
    int MetricTypeId,
    string MetricName,
    string MetricUnit,
    int MeasurementsCount,
    decimal MinMeasuredValue,
    decimal MaxMeasuredValue,
    decimal AverageMeasuredValue);

public sealed record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

