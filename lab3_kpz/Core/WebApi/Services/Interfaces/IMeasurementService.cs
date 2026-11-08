using System.Security.Claims;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Measurements;

namespace lab3_kpz.Core.WebApi.Services.Interfaces;

public interface IMeasurementService
{
    Task<PagedResultDto<MeasurementResponseDto>> GetAllAsync(MeasurementQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<MeasurementResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default);
    Task<MeasurementResponseDto> CreateAsync(CreateMeasurementDto dto, ClaimsPrincipal principal, CancellationToken ct = default);
    Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
}

