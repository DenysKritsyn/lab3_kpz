using Kpz.Lab2.Data;
using Kpz.Lab2.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddUserSecrets<Program>(optional: true);

var connectionString = builder.Configuration.GetConnectionString("SymptomDiaryDb")
    ?? throw new InvalidOperationException("Connection string 'SymptomDiaryDb' is missing.");

bool enableSqlLogging = args.Contains("--sql-log");

builder.Services.AddDbContext<SymptomDiaryDbContext>(options =>
{
    options.UseSqlServer(connectionString)
        .UseSeeding((context, _) => SeedData.Seed(context))
        .UseAsyncSeeding((context, _, ct) => SeedData.SeedAsync(context, ct));

    if (enableSqlLogging)
    {
        options.LogTo(Console.WriteLine, [DbLoggerCategory.Database.Command.Name], LogLevel.Information)
               .EnableDetailedErrors();
    }
    else
    {
        options.UseLoggerFactory(LoggerFactory.Create(_ => { }));
    }
});

builder.Services.AddSingleton(sp => new TaskManager(sp, connectionString));
builder.Logging.ClearProviders();

using var host = builder.Build();
using var cts = new CancellationTokenSource();
var ct = cts.Token;

var taskManager = host.Services.GetRequiredService<TaskManager>();

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("================================================================================");
Console.WriteLine("  KPZ LAB 2: DATA ACCESS WITH ADO.NET AND ENTITY FRAMEWORK CORE 10");
Console.WriteLine("  Variant 15: Symptom Diary");
Console.WriteLine("================================================================================\n");

Console.WriteLine("[Task 2 & 3] Applying migrations and verifying initial seed data...");
await taskManager.RunMigrationAndVerifySeedDataAsync(ct);

Console.WriteLine("[Task 4] Running ADO.NET CRUD demonstration (DiaryEntries & Measurements)...");
await taskManager.RunAdoNetCrudDemoAsync(ct);

Console.WriteLine("[Task 5] Running ADO.NET complex queries (Multi-table JOIN & GROUP BY)...");
await taskManager.RunAdoNetComplexQueriesDemoAsync(ct);

Console.WriteLine("[Task 6] Running ADO.NET transaction scenarios (Commit & Rollback across 4 tables)...");
await taskManager.RunAdoNetTransactionsDemoAsync(ct);

Console.WriteLine("[Task 7 & 8] Running EF Core CRUD and complex entity graph transaction...");
await taskManager.RunEfCoreCrudAndGraphTransactionDemoAsync(ct);

Console.WriteLine("[Task 9 & 13] Running 6 LINQ queries, pagination, and SQL diagnostics (ToQueryString)...");
await taskManager.RunLinqQueriesAndSqlDiagnosticsDemoAsync(ct);

Console.WriteLine("[Task 10] Running Optimistic Concurrency demonstration (RowVersion with 2 DbContexts)...");
await taskManager.RunOptimisticConcurrencyDemoAsync(ct);

Console.WriteLine("[Task 11 & 12] Verifying second migration (UserPhoneNumber & Index) and Reverse Engineering (Scaffolded DbContext)...");
await taskManager.RunSecondMigrationAndScaffoldVerificationDemoAsync(ct);

Console.WriteLine("================================================================================");
Console.WriteLine("  All demonstration tasks completed successfully.");
Console.WriteLine("================================================================================");

public partial class Program;
