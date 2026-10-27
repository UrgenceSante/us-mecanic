using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Api.Endpoints;

internal static class VehicleEndpoints
{
    public const long MaxImportFileBytes = 2 * 1024 * 1024;

    public sealed record ChangeStatusRequest(VehicleStatus Status);

    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var vehicles = app.MapGroup("/api/vehicles").WithTags("Véhicules");

        vehicles.MapGet("/", async (string? search, VehicleStatus? status, Guid? type, ListVehicles useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(new VehicleFilter(search, status, type), ct)))
            .WithSummary("Liste du parc, filtrable par texte, statut et type");

        vehicles.MapGet("/{id:guid}", async (Guid id, GetVehicle useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Fiche d'un véhicule");

        vehicles.MapPost("/", async (RegisterVehicleCommand command, RegisterVehicle useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(command, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Ajoute un véhicule au parc");

        vehicles.MapPut("/{id:guid}", async (Guid id, VehicleDetailsInput details, UpdateVehicleDetails useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, details, ct)).ToOk())
            .WithSummary("Modifie les caractéristiques d'un véhicule");

        vehicles.MapPut("/{id:guid}/status", async (Guid id, ChangeStatusRequest request, ChangeVehicleStatus useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Status, ct)).ToOk())
            .WithSummary("Met en service, immobilise ou sort un véhicule du parc");

        vehicles.MapPost("/import", ImportAsync)
            .DisableAntiforgery() // API authentifiée par jeton Bearer, pas par cookie
            .WithSummary("Importe un parc existant depuis un fichier CSV (dryRun=true pour simuler)");

        return app;
    }

    private static async Task<IResult> ImportAsync(IFormFile file, bool? dryRun, ImportVehicles useCase, CancellationToken ct)
    {
        if (file.Length > MaxImportFileBytes)
        {
            return TypedResults.Problem(
                title: $"Le fichier dépasse la taille maximale de {MaxImportFileBytes / 1024 / 1024} Mo.",
                statusCode: StatusCodes.Status413PayloadTooLarge,
                extensions: new Dictionary<string, object?> { ["code"] = "import.file_too_large" });
        }

        await using var content = file.OpenReadStream();
        return (await useCase.ExecuteAsync(content, dryRun ?? false, ct)).ToOk();
    }
}
