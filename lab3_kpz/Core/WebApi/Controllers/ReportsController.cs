using FluentValidation;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Reports;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lab3_kpz.Core.WebApi.Controllers;

/// <summary>
/// Manages aggregated medical summary reports generated from patient symptom diary entries.
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
[Produces("application/json")]
public sealed class ReportsController(
    IReportService reportService,
    IAuthorizationService authorizationService,
    IValidator<CreateReportDto> createValidator) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of generated medical reports.
    /// </summary>
    /// <param name="query">Pagination and period filter parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the paginated list of reports.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType<PagedResultDto<ReportResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResultDto<ReportResponseDto>>> GetAll(
        [FromQuery] ReportQueryDto query,
        CancellationToken ct)
    {
        var result = await reportService.GetAllAsync(query, User, ct);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific medical report by its unique identifier (accessible by owner, Doctor, or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the report.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the requested medical report.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not the owner, a Doctor, or an Admin.</response>
    /// <response code="404">If the report does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ReportResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponseDto>> GetById(int id, CancellationToken ct)
    {
        var ownerId = await reportService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Report", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerDoctorOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var report = await reportService.GetByIdAsync(id, User, ct);
        return Ok(report);
    }

    /// <summary>
    /// Generates and persists a new summary report for the authenticated user over the specified date period.
    /// </summary>
    /// <param name="dto">The report period payload.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Report generated and saved successfully.</response>
    /// <response code="400">If the date period is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<ReportResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ReportResponseDto>> Create(
        [FromBody] CreateReportDto dto,
        CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);
        var created = await reportService.CreateAsync(dto, User, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.ReportId }, created);
    }

    /// <summary>
    /// Deletes a medical report by its unique identifier (accessible only by the owner or Admin).
    /// </summary>
    /// <param name="id">The unique identifier of the report to delete.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Report deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is neither the owner nor an Admin.</response>
    /// <response code="404">If the report does not exist.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var ownerId = await reportService.GetOwnerIdAsync(id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Report", id);

        var authResult = await authorizationService.AuthorizeAsync(
            User,
            new OwnedResource(ownerId),
            AppPolicies.OwnerOrAdmin);

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        await reportService.DeleteAsync(id, User, ct);
        return NoContent();
    }
}

