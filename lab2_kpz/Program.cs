using lab2_kpz.Core.DemonstrativeScenarios;
using lab2_kpz.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

if (Environment.GetCommandLineArgs().Any(a => a.Contains("ef.dll") || a.Contains("dotnet-ef")))
{
    return;
}

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var connectionString = configuration.GetConnectionString("lab2-kpz-connection-string")
    ?? throw new InvalidOperationException("Connection string 'lab2-kpz-connection-string' was not found in User Secrets.");

var options = new DbContextOptionsBuilder<SymptomDiaryDbContext>()
    .UseSqlServer(connectionString)
    .UseSeeding((context, _) => SeedData.ConfigureRepresentativeData(context))
    .UseAsyncSeeding((context, _, ct) => SeedData.ConfigureRepresentativeDataAsync(context, ct))
    .Options;

await using var context = new SymptomDiaryDbContext(options);

// Ensure migrations and seeding are applied
await context.Database.MigrateAsync();

var demo = new ProgramDemonstration(connectionString);

// 1. Initial / Current Database State
await demo.ShowDatabaseStateAsync();

// 2. ADO.NET CRUD (Users & DiaryEntries)
await demo.RunAdoNetCrudAsync();

// 3. ADO.NET Complex SQL Queries (JOIN + GROUP BY)
await demo.RunComplexSqlQueriesAsync();

// 4. ADO.NET Transaction (Commit & Rollback)
await demo.RunAdoNetTransactionScenarioAsync();

// 5. EF Core CRUD (DiaryEntries, DayIntervals, DayEvents)
await demo.RunEfCoreCrudAsync();

// 6. EF Core Complex Business Operation with Entity Graph (CreateReport)
await demo.RunEfCoreComplexBusinessOperationAsync();

// 7. Pagination (Skip + Take)
await demo.RunPaginationAsync();

// 8. Optimistic Concurrency Conflict (Database Wins)
await demo.RunConcurrencyConflictScenarioAsync();

// 9. SQL Logging & ToQueryString Analysis
await demo.RunSqlLoggingScenarioAsync();
