using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Symptoms;

namespace lab3_kpz.Core.WebApi.Services.Interfaces;

public interface ISymptomService
{
    Task<PagedResultDto<SymptomResponseDto>> GetAllAsync(SymptomQueryDto query, CancellationToken ct = default);
    Task<SymptomResponseDto> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SymptomResponseDto> CreateAsync(CreateSymptomDto dto, CancellationToken ct = default);
    Task<SymptomResponseDto> UpdateAsync(int id, UpdateSymptomDto dto, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

