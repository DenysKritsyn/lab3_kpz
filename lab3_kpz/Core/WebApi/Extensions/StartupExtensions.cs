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
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace lab3_kpz.Core.WebApi.Extensions;

public static class StartupExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("lab2-kpz-connection-string")
            ?? throw new InvalidOperationException("Connection string 'lab2-kpz-connection-string' was not found in User Secrets.");

        services.AddDbContext<SymptomDiaryDbContext>(options =>
            options.UseSqlServer(connectionString)
                   .UseSeeding((context, _) => SeedData.ConfigureRepresentativeData(context))
                   .UseAsyncSeeding((context, _, ct) => SeedData.ConfigureRepresentativeDataAsync(context, ct)));

        return services;
    }

    public static IServiceCollection AddWebApiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new()
                {
                    Title = "Symptom Diary API",
                    Version = "v1",
                    Description = "Web API for tracking patient symptoms, daily intervals, events, physiological measurements, and medical summary reports (Variant 15).",
                    Contact = new()
                    {
                        Name = "Software Engineering Department (Lviv Polytechnic)",
                        Url = new Uri("https://pz.lpnu.ua")
                    },
                    License = new() { Name = "MIT" }
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                var authority = configuration["Oidc:Authority"]?.TrimEnd('/')
                    ?? "http://localhost:8080/realms/symptom-diary";

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your Keycloak JWT access token."
                };

                document.Components.SecuritySchemes["OAuth2"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Keycloak OpenID Connect (Authorization Code Flow with PKCE)",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                            TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                ["openid"] = "OpenID Connect scope",
                                ["profile"] = "User profile information",
                                ["email"] = "User email address"
                            }
                        }
                    }
                };

                return Task.CompletedTask;
            });
        });
        services.AddSingleton<SymptomDiaryMapper>();
        services.AddValidatorsFromAssemblyContaining<CreateSymptomDtoValidator>();
        services.AddMemoryCache();

        services.AddOutputCache(options =>
        {
            options.AddPolicy("SymptomsCache", builder => builder
                .AddPolicy<AuthenticatedOutputCachePolicy>()
                .Expire(TimeSpan.FromSeconds(30))
                .SetVaryByQuery("page", "pageSize", "search", "sortBy", "sortDirection")
                .Tag("symptoms"), excludeDefaultPolicy: true);
        });

        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISymptomService, SymptomService>();
        services.AddScoped<IDiaryEntryService, DiaryEntryService>();
        services.AddScoped<IDayEventService, DayEventService>();
        services.AddScoped<IMeasurementService, MeasurementService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Oidc:Authority"];
                options.Audience = configuration["Oidc:Audience"];
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "sub",
                    RoleClaimType = "roles"
                };
            });

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
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

        services.AddSingleton<IAuthorizationHandler, OwnerOrAdminHandler>();

        return services;
    }

    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        if (!Environment.GetCommandLineArgs().Any(a => a.Contains("ef.dll") || a.Contains("dotnet-ef")))
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SymptomDiaryDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}

public sealed class AuthenticatedOutputCachePolicy : IOutputCachePolicy
{
    public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var method = context.HttpContext.Request.Method;
        context.EnableOutputCaching = HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
        context.AllowCacheLookup = context.EnableOutputCaching;
        context.AllowCacheStorage = context.EnableOutputCaching;
        context.AllowLocking = true;
        context.CacheVaryByRules.QueryKeys = "*";
        return ValueTask.CompletedTask;
    }

    public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation) =>
        ValueTask.CompletedTask;

    public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
    {
        var response = context.HttpContext.Response;
        if (response.StatusCode != StatusCodes.Status200OK)
        {
            context.AllowCacheStorage = false;
        }
        return ValueTask.CompletedTask;
    }
}

