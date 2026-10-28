using UsMecanic.Application.Maintenance.Garages;

namespace UsMecanic.Api.Endpoints;

internal static class GarageEndpoints
{
    public sealed record GarageRequest(string? Name);

    public static IEndpointRouteBuilder MapGarageEndpoints(this IEndpointRouteBuilder app)
    {
        var garages = app.MapGroup("/api/garages").WithTags("Garages");

        garages.MapGet("/", async (bool? includeInactive, ListGarages useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary("Liste les garages (actifs uniquement, sauf includeInactive=true)");

        garages.MapPost("/", async (GarageRequest request, CreateGarage useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.Name, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/garages/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Crée un garage");

        garages.MapPut("/{id:guid}", async (Guid id, GarageRequest request, RenameGarage useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Name, ct)).ToOk())
            .WithSummary("Renomme un garage");

        garages.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateGarage useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Désactive un garage (il n'est plus proposé mais reste affiché sur les interventions)");

        return app;
    }
}
