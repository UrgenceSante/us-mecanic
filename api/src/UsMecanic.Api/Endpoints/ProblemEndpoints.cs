using UsMecanic.Application.Problems;

namespace UsMecanic.Api.Endpoints;

internal static class ProblemEndpoints
{
    public static IEndpointRouteBuilder MapProblemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/problems", async (ReportProblemInput input, ReportProblem useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{result.Value.VehicleId}/problems", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithTags("Problèmes")
            .WithSummary("Signale un problème sur un véhicule désigné par son immatriculation (statut « Signalé »)");

        app.MapGet("/api/vehicles/{vehicleId:guid}/problems", async (Guid vehicleId, ListVehicleProblems useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithTags("Problèmes")
            .WithSummary("Problèmes d'un véhicule, du plus récent au plus ancien");

        app.MapGet("/api/problems/{problemId:guid}", async (Guid problemId, GetProblem useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(problemId, ct)).ToOk())
            .WithTags("Problèmes")
            .WithSummary("Détail d'un problème, avec son analyse s'il est analysé");

        app.MapPut("/api/problems/{problemId:guid}/analysis", async (Guid problemId, AnalyzeProblemInput input, AnalyzeProblem useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(problemId, input, ct)).ToOk())
            .WithTags("Problèmes")
            .WithSummary("Analyse un problème signalé (nature, élément, texte, immobilisation) : il passe à « Analysé »");

        return app;
    }
}
