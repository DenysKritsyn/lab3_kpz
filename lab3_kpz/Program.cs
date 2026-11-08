using System.Security.Claims;
using lab3_kpz.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddOpenApi();

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

builder.Services.AddAuthorizationBuilder();

var app = builder.Build();

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
