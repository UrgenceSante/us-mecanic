using UsMecanic.Application.Rentals;

namespace UsMecanic.Api.Endpoints;

internal static class RentalEndpoints
{
    public sealed record EndRentalRequest(DateOnly EndDate);

    public static IEndpointRouteBuilder MapRentalEndpoints(this IEndpointRouteBuilder app)
    {
        var rentals = app.MapGroup("/api/vehicles/{vehicleId:guid}/rentals").WithTags("Locations");

        rentals.MapGet("/", async (Guid vehicleId, ListRentals useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithSummary("Locations d'un véhicule, de la plus récente à la plus ancienne");

        rentals.MapPost("/", async (Guid vehicleId, RentalInput input, AddRental useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{vehicleId}/rentals/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Enregistre une location (passée, en cours ou prévue)");

        rentals.MapPut("/{rentalId:guid}", async (Guid vehicleId, Guid rentalId, RentalInput input, UpdateRental useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, rentalId, input, ct)).ToOk())
            .WithSummary("Modifie le locataire et les dates d'une location");

        rentals.MapPost("/{rentalId:guid}/end", async (Guid vehicleId, Guid rentalId, EndRentalRequest request, EndRental useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, rentalId, request.EndDate, ct)).ToOk())
            .WithSummary("Termine une location sans date de fin au jour indiqué");

        rentals.MapDelete("/{rentalId:guid}", async (Guid vehicleId, Guid rentalId, DeleteRental useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, rentalId, ct);
                return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
            })
            .WithSummary("Supprime une location");

        return app;
    }
}
