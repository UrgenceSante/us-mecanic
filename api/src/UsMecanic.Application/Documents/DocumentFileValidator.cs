using UsMecanic.Domain.Common;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Contrôle d'un fichier déposé : type reconnu par la signature des octets (jamais par l'extension), taille de
/// 1 octet à 10 Mo, nom d'origine nettoyé. Les erreurs ne reprennent ni le nom ni le contenu.
/// </summary>
public static class DocumentFileValidator
{
    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public static Result<DocumentFile> Validate(ReadOnlySpan<byte> content, string? originalFileName)
    {
        if (content.Length == 0)
        {
            return Result<DocumentFile>.Failure(DocumentErrors.FileEmpty);
        }

        if (content.Length > Document.MaxSizeBytes)
        {
            return Result<DocumentFile>.Failure(DocumentErrors.FileTooLarge);
        }

        var contentType = Detect(content);
        if (contentType is null)
        {
            return Result<DocumentFile>.Failure(DocumentErrors.FileTypeUnsupported);
        }

        var fileName = SanitizeFileName(originalFileName);
        return fileName is null
            ? Result<DocumentFile>.Failure(DocumentErrors.FileNameRequired)
            : Result<DocumentFile>.Success(new DocumentFile(fileName, contentType.Value, content.Length));
    }

    /// <summary>
    /// Garde le dernier segment d'un chemin (séparateurs / et \), retire les caractères de contrôle, tronque à
    /// 255 caractères. <c>null</c> si rien ne reste (ou si le nom est « . » ou « .. »).
    /// </summary>
    public static string? SanitizeFileName(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var lastSeparator = name.AsSpan().LastIndexOfAny('/', '\\');
        var segment = lastSeparator >= 0 ? name[(lastSeparator + 1)..] : name;
        var cleaned = string.Concat(segment.Where(c => !char.IsControl(c))).Trim();
        if (cleaned.Length > Document.MaxFileNameLength)
        {
            cleaned = cleaned[..Document.MaxFileNameLength];
            if (char.IsHighSurrogate(cleaned[^1]))
            {
                cleaned = cleaned[..^1];
            }
        }

        return cleaned is "" or "." or ".." ? null : cleaned;
    }

    private static DocumentContentType? Detect(ReadOnlySpan<byte> content)
    {
        if (content.StartsWith(PdfSignature))
        {
            return DocumentContentType.Pdf;
        }

        if (content.StartsWith(JpegSignature))
        {
            return DocumentContentType.Jpeg;
        }

        return content.StartsWith(PngSignature) ? DocumentContentType.Png : null;
    }
}
