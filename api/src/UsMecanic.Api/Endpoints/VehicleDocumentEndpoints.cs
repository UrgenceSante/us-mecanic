using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using UsMecanic.Application.Documents;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Api.Endpoints;

/// <summary>
/// Documents d'un véhicule. Tous authentifiés (aucun jeton dans l'URL : le front lit le contenu avec un fetch
/// portant l'en-tête Authorization). Ni nom de fichier ni contenu ne sont journalisés.
/// </summary>
internal static class VehicleDocumentEndpoints
{
    // 10 Mo de fichier + 1 Mo de marge pour l'enveloppe multipart. Au-delà : 413 (ProblemDetails), jamais 500.
    private const long MaxRequestBytes = Document.MaxSizeBytes + (1024 * 1024);

    public static IEndpointRouteBuilder MapVehicleDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var documents = app.MapGroup("/api/vehicles/{vehicleId:guid}/documents").WithTags("Documents des véhicules");

        documents.MapGet("/", async (Guid vehicleId, Guid? categoryId, ListVehicleDocuments useCase, CancellationToken ct) =>
                (await useCase.ExecuteAsync(vehicleId, categoryId, ct)).ToOk())
            .WithSummary("Documents d'un véhicule (métadonnées seulement, du plus récent au plus ancien, filtre categoryId)");

        documents.MapPost("/", AddAsync)
            .DisableAntiforgery() // API authentifiée par jeton Bearer, pas par cookie
            .WithMetadata(new RequestSizeLimitMetadata(MaxRequestBytes))
            .WithSummary("Ajoute un document (multipart : file, categoryId, label facultatif ; PDF, JPEG ou PNG, 10 Mo au plus)");

        documents.MapGet("/{documentId:guid}/content", ReadContentAsync)
            .WithSummary("Contenu d'un document (inline par défaut, attachment avec download=true)");

        documents.MapDelete("/{documentId:guid}", async (Guid vehicleId, Guid documentId, DeleteVehicleDocument useCase, CancellationToken ct) =>
            {
                var result = await useCase.ExecuteAsync(vehicleId, documentId, ct);
                return result.IsSuccess ? TypedResults.NoContent() : result.Error!.ToProblem();
            })
            .WithSummary("Supprime définitivement un document (contenu compris)");

        return app;
    }

    private static async Task<IResult> AddAsync(
        Guid vehicleId,
        [FromForm] IFormFile? file,
        [FromForm] Guid? categoryId,
        [FromForm] string? label,
        AddVehicleDocument useCase,
        CancellationToken ct)
    {
        if (file is null)
        {
            return DocumentErrors.FileRequired.ToProblem();
        }

        // Refus avant lecture d'un fichier manifestement trop gros (le validateur contrôle aussi le contenu reçu).
        if (file.Length > Document.MaxSizeBytes)
        {
            return DocumentErrors.FileTooLarge.ToProblem();
        }

        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await stream.CopyToAsync(buffer, ct);

        // GetBuffer évite une copie : on ne lit que les octets réellement écrits.
        var content = new ReadOnlyMemory<byte>(buffer.GetBuffer(), 0, (int)buffer.Length);
        var result = await useCase.ExecuteAsync(vehicleId, categoryId, label, file.FileName, content, ct);
        return result.IsSuccess
            ? TypedResults.Created($"/api/vehicles/{vehicleId}/documents/{result.Value.Id}/content", result.Value)
            : result.Error!.ToProblem();
    }

    private static async Task<IResult> ReadContentAsync(
        Guid vehicleId, Guid documentId, bool? download, HttpContext http, GetVehicleDocumentContent useCase, CancellationToken ct)
    {
        var result = await useCase.ExecuteAsync(vehicleId, documentId, ct);
        if (result.IsFailure)
        {
            return result.Error!.ToProblem();
        }

        // Le nom d'origine est encodé par SetHttpFileName (filename* en RFC 5987 s'il n'est pas ASCII).
        var disposition = new ContentDispositionHeaderValue(download == true ? "attachment" : "inline");
        disposition.SetHttpFileName(result.Value.FileName);
        http.Response.Headers.ContentDisposition = disposition.ToString();
        http.Response.Headers.XContentTypeOptions = "nosniff";
        http.Response.Headers.CacheControl = "private, no-store"; // données personnelles : pas de cache partagé
        http.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'"; // sans sandbox (PDF dans Chrome)
        return TypedResults.File(result.Value.Content, result.Value.ContentType);
    }

    private sealed class RequestSizeLimitMetadata(long maxBytes) : IRequestSizeLimitMetadata
    {
        public long? MaxRequestBodySize { get; } = maxBytes;
    }
}
