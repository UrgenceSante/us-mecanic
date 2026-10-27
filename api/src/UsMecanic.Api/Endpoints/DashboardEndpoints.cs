using UsMecanic.Application.Dashboard;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Api.Endpoints;

internal static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var dashboard = app.MapGroup("/api/dashboard").WithTags("Tableau de bord");

        dashboard.MapGet("/deadlines", async (Guid? vehicleId, Guid? vehicleTypeId, Guid? deadlineTypeId, DeadlineStatus? status, ListDashboardDeadlines useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(new DashboardDeadlineFilter(vehicleId, vehicleTypeId, deadlineTypeId, status), ct)))
            .WithSummary("Échéances proches et dépassées du parc (dépassées d'abord, puis les plus proches), filtrables par véhicule, type de véhicule, type d'échéance et statut (Overdue ou DueSoon)");

        return app;
    }
}
