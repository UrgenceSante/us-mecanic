using UsMecanic.Application.Deadlines;

namespace UsMecanic.Api.Endpoints;

internal static class DeadlineThresholdsEndpoints
{
    public sealed record DeadlineThresholdsRequest(int? DueSoonDays, int? DueSoonKm);

    public static IEndpointRouteBuilder MapDeadlineThresholdsEndpoints(this IEndpointRouteBuilder app)
    {
        var thresholds = app.MapGroup("/api/deadline-thresholds").WithTags("Seuils d'échéance");

        thresholds.MapGet("/", async (GetDeadlineThresholds useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(ct)))
            .WithSummary("Lit les seuils globaux « échéance proche » (jours et kilomètres)");

        thresholds.MapPut("/", async (DeadlineThresholdsRequest request, UpdateDeadlineThresholds useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(request.DueSoonDays, request.DueSoonKm, ct)).ToOk())
            .WithSummary("Modifie les seuils globaux « échéance proche »");

        return app;
    }
}
