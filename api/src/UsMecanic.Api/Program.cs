using System.Text.Json.Serialization;
using Scalar.AspNetCore;
using UsMecanic.Api.Configuration;
using UsMecanic.Api.Endpoints;
using UsMecanic.Application;
using UsMecanic.Infrastructure;
using UsMecanic.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddKeycloakAuthentication(builder.Configuration)
    .AddFrontCors(builder.Configuration)
    .AddProblemDetails()
    .AddHealthChecks();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

if (AuthOptions.IsDisabled(app.Configuration))
{
    AuthLog.AuthenticationDisabled(app.Logger);
}

if (app.Configuration.GetValue("Database:MigrateOnStartup", defaultValue: true))
{
    await app.Services.MigrateDatabaseAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors(ServiceCollectionExtensions.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.MapSystemEndpoints();
app.MapVehicleEndpoints();
app.MapVehicleTypeEndpoints();
app.MapDeadlineTypeEndpoints();
app.MapDeadlineThresholdsEndpoints();
app.MapMileageReadingEndpoints();
app.MapVehicleDeadlineEndpoints();

await app.RunAsync();

/// <summary>Point d'entrée exposé pour les tests d'intégration (WebApplicationFactory).</summary>
public partial class Program;
