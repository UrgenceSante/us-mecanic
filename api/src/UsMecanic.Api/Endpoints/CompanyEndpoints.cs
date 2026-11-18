using UsMecanic.Application.Companies;

namespace UsMecanic.Api.Endpoints;

internal static class CompanyEndpoints
{
    public sealed record CompanyRequest(string? Name);

    public static IEndpointRouteBuilder MapCompanyEndpoints(this IEndpointRouteBuilder app)
    {
        var companies = app.MapGroup("/api/companies").WithTags("Companies");

        companies.MapGet("/", async (bool? includeInactive, ListCompanies useCase, CancellationToken ct) =>
                TypedResults.Ok(await useCase.ExecuteAsync(includeInactive ?? false, ct)))
            .WithSummary("Liste les sociétés (actives uniquement, sauf includeInactive=true)");

        companies.MapPost("/", async (CompanyRequest request, CreateCompany useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(request.Name, ct);
                return result.IsSuccess
                    ? TypedResults.Created($"/api/companies/{result.Value.Id}", result.Value)
                    : result.Error!.ToProblem();
            })
            .WithSummary("Crée une société");

        companies.MapPut("/{id:guid}", async (Guid id, CompanyRequest request, RenameCompany useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, request.Name, ct)).ToOk())
            .WithSummary("Renomme une société");

        companies.MapPost("/{id:guid}/deactivate", async (Guid id, DeactivateCompany useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(id, ct)).ToOk())
            .WithSummary("Désactive une société (elle n'est plus proposée mais reste affichée sur les véhicules et locations qui la citent)");

        return app;
    }
}
