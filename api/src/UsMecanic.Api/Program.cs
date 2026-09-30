using Scalar.AspNetCore;
using UsMecanic.Api.Configuration;
using UsMecanic.Api.Endpoints;
using UsMecanic.Application;
using UsMecanic.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddKeycloakAuthentication(builder.Configuration)
    .AddFrontCors(builder.Configuration)
    .AddProblemDetails()
    .AddHealthChecks();

builder.Services.AddOpenApi();

var app = builder.Build();

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

await app.RunAsync();

/// <summary>Point d'entrée exposé pour les tests d'intégration (WebApplicationFactory).</summary>
public partial class Program;
