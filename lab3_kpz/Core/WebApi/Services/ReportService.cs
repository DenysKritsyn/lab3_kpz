using System.Security.Claims;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.DTOs.Common;
using lab3_kpz.Core.WebApi.DTOs.Reports;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Mappings;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Data;
using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;

namespace lab3_kpz.Core.WebApi.Services;

public sealed class ReportService(
    SymptomDiaryDbContext db,
    SymptomDiaryMapper mapper,
    IUserService userService) : IReportService
{
    public async Task<PagedResultDto<ReportResponseDto>> GetAllAsync(ReportQueryDto query, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var currentUser = await userService.GetOrCreateCurrentUserAsync(principal, ct);
        var isAdminOrDoctor = principal.IsInRole(AppRoles.Admin) || principal.IsInRole(AppRoles.Doctor);

        var dbQuery = db.Reports
            .AsNoTracking()
            .AsQueryable();

        if (!isAdminOrDoctor)
        {
            dbQuery = dbQuery.Where(r => r.UserId == currentUser.UserId);
        }

        if (query.FromDate.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.ReportPeriodFrom >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            dbQuery = dbQuery.Where(r => r.ReportPeriodTo <= query.ToDate.Value);
        }

        dbQuery = dbQuery.OrderByDescending(r => r.ReportGeneratedAt);

        var totalCount = await dbQuery.CountAsync(ct);

        var reports = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return PagedResultDto<ReportResponseDto>.Create(
            mapper.ToDto(reports),
            totalCount,
            query.Page,
            query.PageSize);
    }

    public async Task<ReportResponseDto> GetByIdAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var report = await db.Reports
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReportId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Report", id);

        return mapper.ToDto(report);
    }

    public async Task<string?> GetOwnerIdAsync(int id, CancellationToken ct = default)
    {
        var owner = await db.Reports
            .AsNoTracking()
            .Where(r => r.ReportId == id)
            .Select(r => r.User.UserEmail)
            .FirstOrDefaultAsync(ct);

        return owner;
    }

    public async Task<ReportResponseDto> CreateAsync(CreateReportDto dto, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var currentUser = await userService.GetOrCreateCurrentUserAsync(principal, ct);

        // Fetch user diary entries for this period
        var entries = await db.DiaryEntries
            .AsNoTracking()
            .Where(d => d.UserId == currentUser.UserId && d.EntryDate >= dto.ReportPeriodFrom && d.EntryDate <= dto.ReportPeriodTo)
            .Include(d => d.DayIntervals)
                .ThenInclude(i => i.IntervalSymptoms)
                    .ThenInclude(s => s.Symptom)
            .Include(d => d.DayEvents)
            .Include(d => d.Measurements)
                .ThenInclude(m => m.Metrics)
            .ToListAsync(ct);

        var totalDaysLogged = entries.Count;
        var symptomsLoggedCount = entries.SelectMany(d => d.DayIntervals).SelectMany(i => i.IntervalSymptoms).Count();
        var measurementsCount = entries.SelectMany(d => d.Measurements).Count();

        var summaryText = $"Symptom Diary Medical Report for {currentUser.UserName} ({currentUser.UserEmail}). " +
                          $"Period: {dto.ReportPeriodFrom:yyyy-MM-dd} to {dto.ReportPeriodTo:yyyy-MM-dd}. " +
                          $"Total logged days: {totalDaysLogged}, total symptom records: {symptomsLoggedCount}, total measurement points: {measurementsCount}.";

        var report = new Report
        {
            UserId = currentUser.UserId,
            ReportPeriodFrom = dto.ReportPeriodFrom,
            ReportPeriodTo = dto.ReportPeriodTo,
            ReportGeneratedAt = DateTime.UtcNow,
            ReportSummary = summaryText
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync(ct);

        return mapper.ToDto(report);
    }

    public async Task DeleteAsync(int id, ClaimsPrincipal principal, CancellationToken ct = default)
    {
        var report = await db.Reports.FirstOrDefaultAsync(r => r.ReportId == id, ct)
            ?? throw ResourceNotFoundException.ForEntity("Report", id);

        db.Reports.Remove(report);
        await db.SaveChangesAsync(ct);
    }
}

