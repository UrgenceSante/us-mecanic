using UsMecanic.Application.Mileage;

namespace UsMecanic.Api.Endpoints;

internal static class MileageReadingEndpoints
{
    public static IEndpointRouteBuilder MapMileageReadingEndpoints(this IEndpointRouteBuilder app)
    {
        var readings = app.MapGroup("/api/vehicles/{vehicleId:guid}/mileage-readings").WithTags("Relevés kilométriques");

        readings.MapGet("/", async (Guid vehicleId, ListMileageReadings useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithSummary("Historique des relevés d'un véhicule, du plus récent au plus ancien");

        readings.MapPost("/", async (Guid vehicleId, MileageReadingInput input, AddMileageReading useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{vehicleId}/mileage-readings/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Ajoute un relevé (confirmHighMileage=true pour confirmer plus de 1 000 km/jour)");

        readings.MapPut("/{readingId:guid}", async (Guid vehicleId, Guid readingId, MileageReadingInput input, UpdateMileageReading useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, readingId, input, ct)).ToOk())
            .WithSummary("Modifie un relevé (mêmes contrôles qu'à l'ajout)");

        readings.MapDelete("/{readingId:guid}", async (Guid vehicleId, Guid readingId, DeleteMileageReading useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, readingId, ct);
                return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
            })
            .WithSummary("Supprime un relevé");

        return app;
    }
}
