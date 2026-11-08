using lab3_kpz.Core.WebApi.DTOs.Common;

namespace lab3_kpz.Core.WebApi.DTOs.Reports;

public sealed record ReportResponseDto(
    int ReportId,
    int UserId,
    DateOnly ReportPeriodFrom,
    DateOnly ReportPeriodTo,
    DateTime ReportGeneratedAt,
    string ReportSummary);

public sealed record CreateReportDto(
    DateOnly ReportPeriodFrom,
    DateOnly ReportPeriodTo);

public sealed class ReportQueryDto : PaginationQueryDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}