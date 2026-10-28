using UsMecanic.Application.Deadlines;

namespace UsMecanic.Api.Endpoints;

internal static class VehicleDeadlineEndpoints
{
    public static IEndpointRouteBuilder MapVehicleDeadlineEndpoints(this IEndpointRouteBuilder app)
    {
        var deadlines = app.MapGroup("/api/vehicles/{vehicleId:guid}/deadlines").WithTags("Échéances des véhicules");

        deadlines.MapGet("/", async (Guid vehicleId, ListVehicleDeadlines useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithSummary("Échéances d'un véhicule avec leur statut (en cours d'abord, par urgence)");

        deadlines.MapPost("/", async (Guid vehicleId, VehicleDeadlineInput input, AddVehicleDeadline useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{vehicleId}/deadlines/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Ajoute une échéance (périodique : dernière réalisation ; ponctuelle : date et/ou km d'échéance)");

        deadlines.MapPut("/{deadlineId:guid}/interval", async (Guid vehicleId, Guid deadlineId, VehicleDeadlineIntervalInput input, UpdateVehicleDeadlineInterval useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, deadlineId, input, ct)).ToOk())
            .WithSummary("Modifie la périodicité d'une échéance et recalcule la prochaine échéance");

        deadlines.MapPost("/{deadlineId:guid}/completions", async (Guid vehicleId, Guid deadlineId, DeadlineCompletionInput input, CompleteVehicleDeadline useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, deadlineId, input, ct)).ToOk())
            .WithSummary("Marque une échéance réalisée (périodique : calcule la suivante ; ponctuelle : la clôt) crée une intervention liée à cette échéance et un relevé si un km est fourni");

        return app;
    }
}
