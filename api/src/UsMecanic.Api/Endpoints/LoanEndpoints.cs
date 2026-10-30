using UsMecanic.Application.Costs.Loans;

namespace UsMecanic.Api.Endpoints;

internal static class LoanEndpoints
{
    public static IEndpointRouteBuilder MapLoanEndpoints(this IEndpointRouteBuilder app)
    {
        var loans = app.MapGroup("/api/vehicles/{vehicleId:guid}/loans").WithTags("Emprunts");

        loans.MapGet("/", async (Guid vehicleId, ListLoans useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, ct)).ToOk())
            .WithSummary("Emprunts d'un véhicule, du plus récent au plus ancien");

        loans.MapPost("/", async (Guid vehicleId, LoanInput input, AddLoan useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, input, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/vehicles/{vehicleId}/loans/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Enregistre un emprunt");

        loans.MapPut("/{loanId:guid}", async (Guid vehicleId, Guid loanId, LoanInput input, UpdateLoan useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, loanId, input, ct)).ToOk())
            .WithSummary("Modifie un emprunt (la durée peut être raccourcie : remboursement anticipé ou vente)");

        loans.MapDelete("/{loanId:guid}", async (Guid vehicleId, Guid loanId, DeleteLoan useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, loanId, ct);
                return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
            })
            .WithSummary("Supprime un emprunt");

        return app;
    }
}
