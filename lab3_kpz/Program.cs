using System.Security.Claims;
using FluentValidation;
using lab3_kpz.Core.WebApi.Authorization;
using lab3_kpz.Core.WebApi.Exceptions;
using lab3_kpz.Core.WebApi.Mappings;
using lab3_kpz.Core.WebApi.Services;
using lab3_kpz.Core.WebApi.Services.Interfaces;
using lab3_kpz.Core.WebApi.Validators;
using lab3_kpz.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("lab2-kpz-connection-string")
    ?? throw new InvalidOperationException("Connection string 'lab2-kpz-connection-string' was not found in User Secrets.");

builder.Services.AddDbContext<SymptomDiaryDbContext>(options =>
    options.UseSqlServer(connectionString)
           .UseSeeding((context, _) => SeedData.ConfigureRepresentativeData(context))
           .UseAsyncSeeding((context, _, ct) => SeedData.ConfigureRepresentativeDataAsync(context, ct)));

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<SymptomDiaryMapper>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSymptomDtoValidator>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISymptomService, SymptomService>();
builder.Services.AddScoped<IDiaryEntryService, DiaryEntryService>();
builder.Services.AddScoped<IDayEventService, DayEventService>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Oidc:Authority"];
        options.Audience = builder.Configuration["Oidc:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = "roles"
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppPolicies.RequireAdminOnly, policy => policy
        .RequireAuthenticatedUser()
        .RequireRole(AppRoles.Admin))
    .AddPolicy(AppPolicies.RequireDoctorOrAdmin, policy => policy
        .RequireAuthenticatedUser()
        .RequireRole(AppRoles.Doctor, AppRoles.Admin))
    .AddPolicy(AppPolicies.OwnerOrAdmin, policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new OwnerOrAdminRequirement(allowDoctor: false)))
    .AddPolicy(AppPolicies.OwnerDoctorOrAdmin, policy => policy
        .RequireAuthenticatedUser()
        .AddRequirements(new OwnerOrAdminRequirement(allowDoctor: true)));




builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrAdminHandler>();

var app = builder.Build();

app.UseExceptionHandler();

if (!Environment.GetCommandLineArgs().Any(a => a.Contains("ef.dll") || a.Contains("dotnet-ef")))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
