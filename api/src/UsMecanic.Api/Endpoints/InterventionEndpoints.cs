using UsMecanic.Application.Maintenance.Interventions;

namespace UsMecanic.Api.Endpoints;

internal static class InterventionEndpoints
{
    public static IEndpointRouteBuilder MapInterventionEndpoints(this IEndpointRouteBuilder app)
    {
        var interventions = app.MapGroup("/api/vehicles/{vehicleId:guid}/interventions").WithTags("Carnet d'entretien");

        interventions.MapGet("/", async (Guid vehicleId, ListInterventions useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithSummary("Carnet d'entretien d'un véhicule, de l'intervention la plus récente à la plus ancienne");

        interventions.MapPost("/", async (Guid vehicleId, InterventionInput input, AddIntervention useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{vehicleId}/interventions/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Ajoute une intervention : réalise les échéances cochées et crée un relevé si un km est fourni (une seule transaction)");

        interventions.MapDelete("/{interventionId:guid}", async (Guid vehicleId, Guid interventionId, DeleteIntervention useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, interventionId, ct);
                return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
            })
            .WithSummary("Supprime une intervention et ses liens ; les échéances ne sont pas recalculées et le relevé kilométrique est conservé");

        return app;
    }
}
