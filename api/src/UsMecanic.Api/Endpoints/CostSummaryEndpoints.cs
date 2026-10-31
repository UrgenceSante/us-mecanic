using UsMecanic.Application.Costs.Summary;

namespace UsMecanic.Api.Endpoints;

internal static class CostSummaryEndpoints
{
    public static IEndpointRouteBuilder MapCostSummaryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/costs/summary", async (DateOnly? from, DateOnly? to, string? granularity, Guid? vehicleId, GetCostSummary useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(new CostSummaryQuery(from, to, granularity, vehicleId), ct)).ToOk())
            .WithTags("Coûts")
            .WithSummary("Synthèse des coûts du parc ou d'un véhicule, par mois ou par année");

        return app;
    }
}
