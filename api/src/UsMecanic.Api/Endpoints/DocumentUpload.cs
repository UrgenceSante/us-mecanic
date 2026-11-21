using Microsoft.AspNetCore.Http.Metadata;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Api.Endpoints;

/// <summary>
/// Lecture commune des envois de fichier (documents du véhicule et factures) : contrôle de la taille avant lecture,
/// copie sans duplication du contenu, décodage du nom. Ni nom de fichier ni contenu ne sont journalisés.
/// </summary>
internal static class DocumentUpload
{
    // 10 Mo de fichier + 1 Mo de marge pour l'enveloppe multipart. Au-delà : 413 (ProblemDetails), jamais 500.
    private const long MaxRequestBytes = Document.MaxSizeBytes + (1024 * 1024);

    /// <summary>Limite de taille de la requête, à poser sur les endpoints d'envoi.</summary>
    public static IRequestSizeLimitMetadata SizeLimit { get; } = new RequestSizeLimitMetadata(MaxRequestBytes);

    public static async Task<Result<UploadedFile>> ReadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return Result<UploadedFile>.Failure(DocumentErrors.FileRequired);
        }

        // Refus avant lecture d'un fichier manifestement trop gros (le validateur contrôle aussi le contenu reçu).
        if (file.Length > Document.MaxSizeBytes)
        {
            return Result<UploadedFile>.Failure(DocumentErrors.FileTooLarge);
        }

        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await stream.CopyToAsync(buffer, cancellationToken);

        // GetBuffer évite une copie : on ne lit que les octets réellement écrits.
        return Result<UploadedFile>.Success(new(
            UnescapeQuoted(file.FileName), new ReadOnlyMemory<byte>(buffer.GetBuffer(), 0, (int)buffer.Length)));
    }

    // Le nom du multipart est une quoted-string : on décode seulement \" et \\ (les autres \ restent, pour les
    // anciens navigateurs qui envoient un chemin Windows). Le validateur garde ensuite le dernier segment du chemin.
    private static string UnescapeQuoted(string fileName) =>
        fileName.Replace("\\\"", "\"", StringComparison.Ordinal).Replace("\\\\", "\\", StringComparison.Ordinal);

    private sealed class RequestSizeLimitMetadata(long maxBytes) : IRequestSizeLimitMetadata
    {
        public long? MaxRequestBodySize { get; } = maxBytes;
    }
}

/// <summary>Fichier reçu : nom d'origine (non nettoyé) et contenu.</summary>
internal readonly record struct UploadedFile(string FileName, ReadOnlyMemory<byte> Content);
