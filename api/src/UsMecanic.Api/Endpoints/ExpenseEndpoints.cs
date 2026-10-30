using UsMecanic.Application.Costs.Expenses;

namespace UsMecanic.Api.Endpoints;

internal static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var expenses = app.MapGroup("/api/vehicles/{vehicleId:guid}/expenses").WithTags("Dépenses");

        expenses.MapGet("/", async (Guid vehicleId, ListExpenses useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithSummary("Dépenses d'un véhicule, de la plus récente à la plus ancienne");

        expenses.MapPost("/", async (Guid vehicleId, ExpenseInput input, AddExpense useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{vehicleId}/expenses/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Ajoute une dépense ponctuelle (catégorie active obligatoire ; l'utilisation courante n'est pas saisissable)");

        expenses.MapPut("/{expenseId:guid}", async (Guid vehicleId, Guid expenseId, ExpenseInput input, UpdateExpense useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, expenseId, input, ct)).ToOk())
            .WithSummary("Modifie une dépense manuelle (mêmes contrôles qu'à l'ajout)");

        expenses.MapDelete("/{expenseId:guid}", async (Guid vehicleId, Guid expenseId, DeleteExpense useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, expenseId, ct);
                return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
            })
            .WithSummary("Supprime une dépense manuelle");

        return app;
    }
}
