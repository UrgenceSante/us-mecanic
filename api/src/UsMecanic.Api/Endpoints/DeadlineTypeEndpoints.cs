using UsMecanic.Application.Deadlines;

namespace UsMecanic.Api.Endpoints;

internal static class DeadlineTypeEndpoints
{
    public sealed record DeadlineTypeRequest(string? Name, int? DefaultIntervalMonths, int? DefaultIntervalKm, int? DueSoonDays, int? DueSoonKm)
    {
        public DeadlineTypeInput ToInput() => new(Name, DefaultIntervalMonths, DefaultIntervalKm, DueSoonDays, DueSoonKm);
    }

    public sealed record AssignRequest(IReadOnlyCollection<Guid>? VehicleTypeIds, DateOnly? LastDoneDate);

    public static IEndpointRouteBuilder MapDeadlineTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var types = app.MapGroup("/api/deadline-types").WithTags("Types d'échéance");

        types.MapGet("/", async (bool? includeInactive, ListDeadlineTypes useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary("Liste les types d'échéance (actifs uniquement, sauf includeInactive=true)");

        types.MapPost("/", async (DeadlineTypeRequest request, CreateDeadlineType useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.ToInput(), ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/deadline-types/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Crée un type d'échéance (sans périodicité, il est ponctuel)");

        types.MapPut("/{id:guid}", async (Guid id, DeadlineTypeRequest request, UpdateDeadlineType useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.ToInput(), ct)).ToOk())
            .WithSummary("Modifie un type d'échéance (nom, périodicité par défaut, seuils)");

        types.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateDeadlineType useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Désactive un type d'échéance (il n'est plus proposé mais reste affiché sur les échéances existantes)");

        types.MapPost("/{id:guid}/assign", async (Guid id, AssignRequest request, AssignDeadlineType useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.VehicleTypeIds, request.LastDoneDate, ct)).ToOk())
            .WithSummary("Ajoute le type d'échéance (périodicité par défaut) aux véhicules d'un ou plusieurs types de véhicule ; renvoie le bilan");

        return app;
    }
}
