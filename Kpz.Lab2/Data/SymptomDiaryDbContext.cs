using Kpz.Lab2.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kpz.Lab2.Data;

public sealed class SymptomDiaryDbContext(DbContextOptions<SymptomDiaryDbContext> options)
    : DbContext(options)
{
    public DbSet<WellbeingLevel> WellbeingLevels => Set<WellbeingLevel>();
    public DbSet<SeverityLevel> SeverityLevels => Set<SeverityLevel>();
    public DbSet<Symptom> Symptoms => Set<Symptom>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<MetricType> MetricTypes => Set<MetricType>();
    public DbSet<User> Users => Set<User>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<DayInterval> DayIntervals => Set<DayInterval>();
    public DbSet<IntervalSymptom> IntervalSymptoms => Set<IntervalSymptom>();
    public DbSet<DayEvent> DayEvents => Set<DayEvent>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(SymptomDiaryDbContext).Assembly);
}

