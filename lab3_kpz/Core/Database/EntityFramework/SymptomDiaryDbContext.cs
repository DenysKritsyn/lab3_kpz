using lab3_kpz.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace lab3_kpz.Data;

public sealed class SymptomDiaryDbContext : DbContext
{
    public SymptomDiaryDbContext() { }
    public SymptomDiaryDbContext(DbContextOptions<SymptomDiaryDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DiaryEntry> DiaryEntries => Set<DiaryEntry>();
    public DbSet<DayInterval> DayIntervals => Set<DayInterval>();
    public DbSet<ConditionState> ConditionStates => Set<ConditionState>();
    public DbSet<IntervalSymptom> IntervalSymptoms => Set<IntervalSymptom>();
    public DbSet<Symptom> Symptoms => Set<Symptom>();
    public DbSet<SeverityLevel> SeverityLevels => Set<SeverityLevel>();
    public DbSet<DayEvent> DayEvents => Set<DayEvent>();
    public DbSet<EventType> EventTypes => Set<EventType>();
    public DbSet<Measurement> Measurements => Set<Measurement>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<MetricUnit> MetricUnits => Set<MetricUnit>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var connectionString = configuration.GetConnectionString("lab2-kpz-connection-string")
                ?? throw new InvalidOperationException("Connection string 'lab2-kpz-connection-string' was not found in User Secrets.");

            optionsBuilder.UseSqlServer(connectionString);
        }

        optionsBuilder.UseSeeding((context, _) => SeedData.ConfigureRepresentativeData(context))
                      .UseAsyncSeeding((context, _, ct) => SeedData.ConfigureRepresentativeDataAsync(context, ct));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SymptomDiaryDbContext).Assembly);
        SeedData.ConfigureStaticData(modelBuilder);
    }
}