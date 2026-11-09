using FluentValidation;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.DiaryEntries;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lab3_kpz.Core.WebApi.Controllers;

/// <summary>
/// Manages daily patient symptom diary entries and their nested intervals, events, and measurements.
/// </summary>
[ApiController]
[Route("api/v1/diary-entries")]
[Authorize]
[Produces("application/json")]
public sealed class DiaryEntriesController(
    IDiaryEntryService diaryEntryService,
    IAuthorizationService authorizationService,
    IValidator<CreateDiaryEntryDto> createValidator,
    IValidator<UpdateDiaryEntryDto> updateValidator,
    IValidator<DiaryEntryQueryDto> queryValidator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of diary entries (patients see only their own entries; doctors and admins see all).
    /// </summary>
    /// <param name="query">Pagination, date range, symptom filter, and sorting parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the paginated list of diary entry summaries.</response>
    /// <response code="400">If query parameters are invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<DiaryEntrySummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<DiaryEntrySummaryDto>>> GetAll(
        [FromQuery] DiaryEntryQueryDto query,
        CancellationToken ct)
    {
        await queryValidator.ValidateAndThrowAsync(query, ct);
        var result = await diaryEntryService.GetAllAsync(query, User, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a detailed diary entry by its unique identifier (accessible by owner, Doctor, or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the diary entry.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the full diary entry with intervals, events, and measurements.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not the owner, a Doctor, or an Admin.</response>
    /// <response code="404">If the diary entry does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<DiaryEntryResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DiaryEntryResponseDto>> GetById(int id, CancellationToken ct)
    {
        var ownerId = await diaryEntryService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerDoctorOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var entry = await diaryEntryService.GetByIdAsync(id, User, ct);
        return Ok(entry);
    }

    /// <summary>
    /// Creates a new daily diary entry for the currently authenticated user.
    /// </summary>
    /// <param name="dto">The diary entry creation payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Diary entry created successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="409">If a diary entry for the specified date already exists for this user.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<DiaryEntryResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DiaryEntryResponseDto>> Create(
        [FromBody] CreateDiaryEntryDto dto,
        CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var created = await diaryEntryService.CreateAsync(dto, User, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.DiaryEntryId }, created);
    }

    /// <summary>
    /// Updates an existing diary entry (accessible only by the entry owner or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the diary entry to update.</param>
    /// <param name="dto">The updated diary entry data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Diary entry updated successfully.</response>
    /// <response code="400">If the request payload fails validation.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner nor an Admin.</response>
    /// <response code="404">If the diary entry does not exist.</response>
    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDiaryEntryDto dto,
        CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(dto, ct);

        var ownerId = await diaryEntryService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await diaryEntryService.UpdateAsync(id, dto, User, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a diary entry by its unique identifier (accessible only by the entry owner or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the diary entry to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Diary entry deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner nor an Admin.</response>
    /// <response code="404">If the diary entry does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ownerId = await diaryEntryService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("DiaryEntry", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await diaryEntryService.DeleteAsync(id, User, ct);
        return NoContent();
    }
}

