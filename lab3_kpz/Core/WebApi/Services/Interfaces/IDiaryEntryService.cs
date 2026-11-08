using System.Security.Claims;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DiaryEntries;

namespace lab3_kpz.Core.WebApi.Services.Interfaces;

public interface IDiaryEntryService
{
    Task<PagedResultDto<DiaryEntrySummaryDto>> GetAllAsync(DiaryEntryQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<DiaryEntryResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default);
    Task<DiaryEntryResponseDto> CreateAsync(CreateDiaryEntryDto dto, ClaimsPrincipal principal, CancellationToken ct = default);
    Task<DiaryEntryResponseDto> UpdateAsync(int id, UpdateDiaryEntryDto dto, ClaimsPrincipal principal, CancellationToken ct = default);
    Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default);
}

