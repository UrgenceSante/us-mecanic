using Microsoft.AspNetCore.Mvc;
using UsMecanic.Application.Documents;
using UsMecanic.Domain.Common;

namespace UsMecanic.Api.Endpoints;

/// <summary>
/// Factures des interventions et des dépenses. Tous authentifiés. L'ouverture, le téléchargement et la suppression
/// d'une facture passent par les endpoints de documents du véhicule (/api/vehicles/{id}/documents/{documentId}...).
/// </summary>
internal static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var interventions = app.MapGroup("/api/vehicles/{vehicleId:guid}/interventions/{interventionId:guid}/invoices")
            .WithTags("Factures");

        interventions.MapGet("/", async (Guid vehicleId, Guid interventionId, ListInvoices useCase, CancellationToken ct) =>
                (await useCase.ForInterventionAsync(vehicleId, interventionId, ct)).ToOk())
            .WithSummary("Factures d'une intervention (métadonnées seulement, du plus récent au plus ancien)");

        interventions.MapPost("/", AddForInterventionAsync)
            .DisableAntiforgery() // API authentifiée par jeton Bearer, pas par cookie
            .WithMetadata(DocumentUpload.SizeLimit)
            .WithSummary("Ajoute une facture à une intervention (multipart : file, label facultatif ; PDF, JPEG ou PNG, 10 Mo au plus)");

        var expenses = app.MapGroup("/api/vehicles/{vehicleId:guid}/expenses/{expenseId:guid}/invoices")
            .WithTags("Factures");

        expenses.MapGet("/", async (Guid vehicleId, Guid expenseId, ListInvoices useCase, CancellationToken ct) =>
                (await useCase.ForExpenseAsync(vehicleId, expenseId, ct)).ToOk())
            .WithSummary("Factures d'une dépense (métadonnées seulement, du plus récent au plus ancien)");

        expenses.MapPost("/", AddForExpenseAsync)
            .DisableAntiforgery()
            .WithMetadata(DocumentUpload.SizeLimit)
            .WithSummary("Ajoute une facture à une dépense ponctuelle saisie à la main (multipart : file, label facultatif ; PDF, JPEG ou PNG, 10 Mo au plus)");

        return app;
    }

    private static async Task<IResult> AddForInterventionAsync(
        Guid vehicleId, Guid interventionId, [FromForm] IFormFile? file, [FromForm] string? label, AddInvoice useCase, CancellationToken ct)
    {
        var upload = await DocumentUpload.ReadAsync(file, ct);
        if (upload.IsFailure)
        {
            return upload.Error!.ToProblem();
        }

        var result = await useCase.ForInterventionAsync(vehicleId, interventionId, label, upload.Value.FileName, upload.Value.Content, ct);
        return Created(vehicleId, result);
    }

    private static async Task<IResult> AddForExpenseAsync(
        Guid vehicleId, Guid expenseId, [FromForm] IFormFile? file, [FromForm] string? label, AddInvoice useCase, CancellationToken ct)
    {
        var upload = await DocumentUpload.ReadAsync(file, ct);
        if (upload.IsFailure)
        {
            return upload.Error!.ToProblem();
        }

        var result = await useCase.ForExpenseAsync(vehicleId, expenseId, label, upload.Value.FileName, upload.Value.Content, ct);
        return Created(vehicleId, result);
    }

    private static IResult Created(Guid vehicleId, Result<DocumentDto> result) =>
        result.IsSuccess
            ? TypedResults.Created($"/api/vehicles/{vehicleId}/documents/{result.Value.Id}/content", result.Value)
            : result.Error!.ToProblem();
}
