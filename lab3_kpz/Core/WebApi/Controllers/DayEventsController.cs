using FluentValidation;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DayEvents;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lab3_kpz.Core.WebApi.Controllers;

/// <summary>
/// Manages daily events recorded within patient symptom diary entries (e.g., medication, meals, physical activity).
/// </summary>
[ApiController]
[Route("api/v1/day-events")]
[Authorize]
[Produces("application/json")]
public sealed class DayEventsController(
    IDayEventService dayEventService,
    IDiaryEntryService diaryEntryService,
    IAuthorizationService authorizationService,
    IValidator<CreateDayEventDto> createValidator,
    IValidator<UpdateDayEventDto> updateValidator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of day events with optional filtering and sorting.
    /// </summary>
    /// <param name="query">Pagination, diary entry filter, event type filter, and sorting parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the paginated list of day events.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<DayEventResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<DayEventResponseDto>>> GetAll(
        [FromQuery] DayEventQueryDto query,
        CancellationToken ct)
    {
        var result = await dayEventService.GetAllAsync(query, User, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific day event by its unique identifier (accessible by owner, Doctor, or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the day event.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the requested day event.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not the owner, a Doctor, or an Admin.</response>
    /// <response code="404">If the day event does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<DayEventResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DayEventResponseDto>> GetById(int id, CancellationToken ct)
    {
        var ownerId = await dayEventService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DayEvent", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerDoctorOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var dayEvent = await dayEventService.GetByIdAsync(id, User, ct);
        return Ok(dayEvent);
    }

    /// <summary>
    /// Creates a new day event inside an existing diary entry (accessible by the diary entry owner or Admin).
    /// </summary>
    /// <param name="dto">The day event creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Day event created successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner of the diary entry nor an Admin.</response>
    /// <response code="404">If the target diary entry or event type does not exist.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<DayEventResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DayEventResponseDto>> Create(
        [FromBody] CreateDayEventDto dto,
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

        var created = await dayEventService.CreateAsync(dto, User, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.DayEventId }, created);
    }

    /// <summary>
    /// Updates an existing day event (accessible only by the owner or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the day event to update.</param>
    /// <param name="dto">The updated day event data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Day event updated successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner nor an Admin.</response>
    /// <response code="404">If the day event or event type does not exist.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDayEventDto dto,
        CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);

        var ownerId = await dayEventService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DayEvent", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await dayEventService.UpdateAsync(id, dto, User, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a day event by its unique identifier (accessible only by the owner or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the day event to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Day event deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner nor an Admin.</response>
    /// <response code="404">If the day event does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ownerId = await dayEventService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DayEvent", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await dayEventService.DeleteAsync(id, User, ct);
        return NoContent();
    }
}

