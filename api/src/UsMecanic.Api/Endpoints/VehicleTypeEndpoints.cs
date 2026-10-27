using UsMecanic.Application.Vehicles.Types;

namespace UsMecanic.Api.Endpoints;

internal static class VehicleTypeEndpoints
{
    public sealed record VehicleTypeRequest(string? Name);

    public static IEndpointRouteBuilder MapVehicleTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var types = app.MapGroup("/api/vehicle-types").WithTags("Types de véhicule");

        types.MapGet("/", async (bool? includeInactive, ListVehicleTypes useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary("Liste les types de véhicule (actifs uniquement, sauf includeInactive=true)");

        types.MapPost("/", async (VehicleTypeRequest request, CreateVehicleType useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.Name, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicle-types/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Crée un type de véhicule");

        types.MapPut("/{id:guid}", async (Guid id, VehicleTypeRequest request, RenameVehicleType useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Name, ct)).ToOk())
            .WithSummary("Renomme un type de véhicule");

        types.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateVehicleType useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Désactive un type de véhicule (il n'est plus proposé mais reste affiché sur les véhicules)");

        return app;
    }
}
