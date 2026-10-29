using UsMecanic.Application.Problems.References;
using UsMecanic.Domain.Problems;

namespace UsMecanic.Api.Endpoints;

/// <summary>
/// Endpoints des cinq référentiels administrables des problèmes. Ils partagent le même contrat que les garages :
/// lister (<c>includeInactive</c>), créer, renommer, désactiver ; aucune suppression.
/// </summary>
internal static class ProblemReferenceEndpoints
{
    public sealed record ProblemReferenceRequest(string? Name);

    public static IEndpointRouteBuilder MapProblemReferenceEndpoints(this IEndpointRouteBuilder app)
    {
        MapReference<ProblemNature>(app, "/api/problem-natures", "Natures de problème");
        MapReference<ProblemElement>(app, "/api/problem-elements", "Éléments concernés");
        MapReference<ProblemActor>(app, "/api/problem-actors", "Acteurs");
        MapReference<ProblemActionType>(app, "/api/problem-action-types", "Types d'action");
        MapReference<ProblemConstraint>(app, "/api/problem-constraints", "Contraintes");
        return app;
    }

    private static void MapReference<T>(IEndpointRouteBuilder app, string route, string tag)
        where T : ProblemReference, IProblemReference<T>
    {
        var group = app.MapGroup(route).WithTags(tag);

        group.MapGet("/", async (bool? includeInactive, ListProblemReferences<T> useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary($"Liste les valeurs du référentiel « {tag} » (actives uniquement, sauf includeInactive=true)");

        group.MapPost("/", async (ProblemReferenceRequest request, CreateProblemReference<T> useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.Name, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"{route}/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary($"Crée une valeur du référentiel « {tag} »");

        group.MapPut("/{id:guid}", async (Guid id, ProblemReferenceRequest request, RenameProblemReference<T> useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Name, ct)).ToOk())
            .WithSummary($"Renomme une valeur du référentiel « {tag} »");

        group.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateProblemReference<T> useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary($"Désactive une valeur du référentiel « {tag} » (plus proposée pour une nouvelle saisie, conservée là où elle est utilisée)");
    }
}
