using UsMecanic.Application.Costs.ExpenseCategories;

namespace UsMecanic.Api.Endpoints;

internal static class ExpenseCategoryEndpoints
{
    public sealed record ExpenseCategoryRequest(string? Name);

    public static IEndpointRouteBuilder MapExpenseCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var expenseCategories = app.MapGroup("/api/expense-categories").WithTags("ExpenseCategories");

        expenseCategories.MapGet("/", async (bool? includeInactive, ListExpenseCategories useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary("Liste les catégories (actives uniquement, sauf includeInactive=true)");

        expenseCategories.MapPost("/", async (ExpenseCategoryRequest request, CreateExpenseCategory useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.Name, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/expense-categories/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Crée une catégorie");

        expenseCategories.MapPut("/{id:guid}", async (Guid id, ExpenseCategoryRequest request, RenameExpenseCategory useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Name, ct)).ToOk())
            .WithSummary("Renomme une catégorie");

        expenseCategories.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateExpenseCategory useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Désactive une catégorie (elle n'est plus proposée mais reste affichée sur les dépenses)");

        return app;
    }
}
