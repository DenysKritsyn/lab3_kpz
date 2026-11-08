using lab3_kpz.Core.WebApi.DTOs.Common;

namespace lab3_kpz.Core.WebApi.DTOs.Measurements;

public sealed record MetricResponseDto(
    int MetricId,
    int MeasurementId,
    int MetricUnitId,
    string MetricName,
    decimal MetricValue,
    string? UnitName);

public sealed record CreateMetricDto(
    int MetricUnitId,
    string MetricName,
    decimal MetricValue);

public sealed record MeasurementResponseDto(
    int MeasurementId,
    int DiaryEntryId,
    TimeOnly MeasuringTime,
    IReadOnlyList<MetricResponseDto> Metrics);

public sealed record CreateMeasurementDto(
    int DiaryEntryId,
    TimeOnly MeasuringTime,
    List<CreateMetricDto> Metrics);

public sealed class MeasurementQueryDto : PaginationQueryDto
{
    public int? DiaryEntryId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? MetricUnitId { get; set; }
    public string? MetricName { get; set; }
}