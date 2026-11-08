using System.Security.Claims;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Reports;

namespace lab3_kpz.Core.WebApi.Services.Interfaces;

public interface IReportService
{
    Task<PagedResultDto<ReportResponseDto>> GetAllAsync(ReportQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<ReportResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default);
    Task<ReportResponseDto> CreateAsync(CreateReportDto dto, ClaimsPrincipal principal, CancellationToken ct = default);
    Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
}

