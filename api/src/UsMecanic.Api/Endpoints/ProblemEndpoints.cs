using UsMecanic.Application.Problems;
using UsMecanic.Domain.Problems;

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

        app.MapGet("/api/problems", async (ProblemStatus? status, Guid? vehicleId, Guid? natureId, ListFleetProblems useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(new FleetProblemFilter(status, vehicleId, natureId), ct)))
            .WithTags("Problèmes")
            .WithSummary("Problèmes du parc, du plus au moins bloquant (immobilisants, non analysés, action en retard, autres), filtrables par statut, véhicule et nature ; les résolus n'y figurent que si le statut Resolved est demandé");

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

        app.MapPost("/api/problems/{problemId:guid}/actions", async (Guid problemId, AddProblemActionInput input, AddProblemAction useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(problemId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/problems/{problemId}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithTags("Problèmes")
            .WithSummary("Ajoute une action de résolution (acteur, type, contrainte, échéance, commentaire, intervention) à un problème analysé");

        app.MapPut("/api/problems/{problemId:guid}/actions/{actionId:guid}", async (Guid problemId, Guid actionId, UpdateProblemActionInput input, UpdateProblemAction useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(problemId, actionId, input, ct)).ToOk())
            .WithTags("Problèmes")
            .WithSummary("Modifie l'échéance, le commentaire et l'intervention liée d'une action ouverte");

        app.MapPost("/api/problems/{problemId:guid}/actions/{actionId:guid}/close", async (Guid problemId, Guid actionId, CloseProblemAction useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(problemId, actionId, ct)).ToOk())
            .WithTags("Problèmes")
            .WithSummary("Clôture une action de résolution");

        return app;
    }
}
