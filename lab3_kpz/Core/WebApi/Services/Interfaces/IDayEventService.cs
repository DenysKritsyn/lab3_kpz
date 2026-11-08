using System.Security.Claims;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DayEvents;

namespace lab3_kpz.Core.WebApi.Services.Interfaces;

public interface IDayEventService
{
    Task<PagedResultDto<DayEventResponseDto>> GetAllAsync(DayEventQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<DayEventResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default);
    Task<DayEventResponseDto> CreateAsync(CreateDayEventDto dto, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<DayEventResponseDto> UpdateAsync(int id, UpdateDayEventDto dto, ClaimsPrincipal principal, CancellationToken ct = default);
    Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
}

