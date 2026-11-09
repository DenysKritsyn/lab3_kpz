using lab3_kpz.Core.WebApi.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddUserSecrets<Program>();

builder.Services
    .AddDatabase(builder.Configuration)
    .AddWebApiInfrastructure(builder.Configuration)
    .AddApplicationServices()
    .AddJwtAuthentication(builder.Configuration, builder.Environment)
    .AddAuthorizationPolicies();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

await app.ApplyDatabaseMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();

await app.RunAsync();
