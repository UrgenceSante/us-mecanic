using UsMecanic.Application.Documents.Categories;

namespace UsMecanic.Api.Endpoints;

internal static class DocumentCategoryEndpoints
{
    public sealed record DocumentCategoryRequest(string? Name);

    public static IEndpointRouteBuilder MapDocumentCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var documentCategories = app.MapGroup("/api/document-categories").WithTags("DocumentCategories");

        documentCategories.MapGet("/", async (bool? includeInactive, ListDocumentCategories useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary("Liste les catégories (actives uniquement, sauf includeInactive=true)");

        documentCategories.MapPost("/", async (DocumentCategoryRequest request, CreateDocumentCategory useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.Name, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/document-categories/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Crée une catégorie");

        documentCategories.MapPut("/{id:guid}", async (Guid id, DocumentCategoryRequest request, RenameDocumentCategory useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Name, ct)).ToOk())
            .WithSummary("Renomme une catégorie");

        documentCategories.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateDocumentCategory useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Désactive une catégorie (elle n'est plus proposée mais reste affichée sur les documents déjà classés)");

        return app;
    }
}
