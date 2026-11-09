using FluentValidation;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Symptoms;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace lab3_kpz.Core.WebApi.Controllers;

/// <summary>
/// Manages the medical symptoms catalog.
/// </summary>
[ApiController]
[Route("api/v1/symptoms")]
[Authorize]
[Produces("application/json")]
public sealed class SymptomsController(
    ISymptomService symptomService,
    IOutputCacheStore outputCacheStore,
    IValidator<CreateSymptomDto> createValidator,
    IValidator<UpdateSymptomDto> updateValidator,
    IValidator<SymptomQueryDto> queryValidator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of symptoms with optional filtering and sorting.
    /// </summary>
    /// <param name="query">Pagination, search, and sorting parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the paginated list of symptoms.</response>
    /// <response code="400">If pagination or sorting parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [OutputCache(PolicyName = "SymptomsCache")]
    [ProducesResponseType<PagedResultDto<SymptomResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<SymptomResponseDto>>> GetAll(
        [FromQuery] SymptomQueryDto query,
        CancellationToken ct)
    {
        await queryValidator.ValidateAndThrowAsync(query, ct);
        var result = await symptomService.GetAllAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific symptom by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the symptom.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the requested symptom.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If the symptom with the specified ID does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<SymptomResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SymptomResponseDto>> GetById(int id, CancellationToken ct)
    {
        var symptom = await symptomService.GetByIdAsync(id, ct);
        return Ok(symptom);
    }

    /// <summary>
    /// Creates a new symptom in the catalog (requires Doctor or Admin role).
    /// </summary>
    /// <param name="dto">The symptom creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Symptom created successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the Doctor or Admin role.</response>
    /// <response code="409">If a symptom with the same name already exists.</response>
    [HttpPost]
    [Authorize(Policy = AppPolicies.RequireDoctorOrAdmin)]
    [Consumes("application/json")]
    [ProducesResponseType<SymptomResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SymptomResponseDto>> Create(
        [FromBody] CreateSymptomDto dto,
        CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var created = await symptomService.CreateAsync(dto, ct);
        await outputCacheStore.EvictByTagAsync("symptoms", ct);
        return CreatedAtAction(nameof(GetById), new { id = created.SymptomId }, created);
    }

    /// <summary>
    /// Updates an existing symptom by its unique identifier (requires Doctor or Admin role).
    /// </summary>
    /// <param name="id">The unique identifier of the symptom to update.</param>
    /// <param name="dto">The updated symptom data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Symptom updated successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the Doctor or Admin role.</response>
    /// <response code="404">If the symptom does not exist.</response>
    /// <response code="409">If another symptom with the same name already exists.</response>
    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireDoctorOrAdmin)]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSymptomDto dto,
        CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);
        await symptomService.UpdateAsync(id, dto, ct);
        await outputCacheStore.EvictByTagAsync("symptoms", ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a symptom from the catalog (requires Admin role).
    /// </summary>
    /// <param name="id">The unique identifier of the symptom to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Symptom deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user does not have the Admin role.</response>
    /// <response code="404">If the symptom does not exist.</response>
    /// <response code="409">If the symptom is referenced by existing diary entries.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.RequireAdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await symptomService.DeleteAsync(id, ct);
        await outputCacheStore.EvictByTagAsync("symptoms", ct);
        return NoContent();
    }
}

