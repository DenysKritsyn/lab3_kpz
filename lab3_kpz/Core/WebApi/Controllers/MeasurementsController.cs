using FluentValidation;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Measurements;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lab3_kpz.Core.WebApi.Controllers;

/// <summary>
/// Manages physiological measurement sessions and their associated numerical metrics (e.g., blood pressure, heart rate, temperature).
/// </summary>
[ApiController]
[Route("api/v1/measurements")]
[Authorize]
[Produces("application/json")]
public sealed class MeasurementsController(
    IMeasurementService measurementService,
    IDiaryEntryService diaryEntryService,
    IAuthorizationService authorizationService,
    IValidator<CreateMeasurementDto> createValidator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of measurements with their metrics.
    /// </summary>
    /// <param name="query">Pagination, date range, diary entry, and metric filter parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the paginated list of measurements.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<MeasurementResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<MeasurementResponseDto>>> GetAll(
        [FromQuery] MeasurementQueryDto query,
        CancellationToken ct)
    {
        var result = await measurementService.GetAllAsync(query, User, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific measurement session and its metrics by unique identifier (accessible by owner, Doctor, or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the measurement session.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the requested measurement session.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not the owner, a Doctor, or an Admin.</response>
    /// <response code="404">If the measurement session does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<MeasurementResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeasurementResponseDto>> GetById(int id, CancellationToken ct)
    {
        var ownerId = await measurementService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Measurement", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerDoctorOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var measurement = await measurementService.GetByIdAsync(id, User, ct);
        return Ok(measurement);
    }

    /// <summary>
    /// Records a new measurement session with one or more metrics inside an existing diary entry.
    /// </summary>
    /// <param name="dto">The measurement creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Measurement session created successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner of the diary entry nor an Admin.</response>
    /// <response code="404">If the target diary entry or any metric unit does not exist.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<MeasurementResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MeasurementResponseDto>> Create(
        [FromBody] CreateMeasurementDto dto,
        CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);

        var ownerId = await diaryEntryService.GetOwnerIdAsync(dto.DiaryEntryId, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", dto.DiaryEntryId);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var created = await measurementService.CreateAsync(dto, User, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.MeasurementId }, created);
    }

    /// <summary>
    /// Deletes a measurement session and its metrics by unique identifier (accessible only by the owner or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the measurement session to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Measurement session deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner nor an Admin.</response>
    /// <response code="404">If the measurement session does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ownerId = await measurementService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Measurement", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await measurementService.DeleteAsync(id, User, ct);
        return NoContent();
    }
}

