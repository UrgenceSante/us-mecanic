using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>Type MIME servi pour chaque type de document accepté.</summary>
public static class DocumentMimeTypes
{
    public static string From(DocumentContentType type) => type switch
    {
        DocumentContentType.Pdf => "application/pdf",
        DocumentContentType.Jpeg => "image/jpeg",
        _ => "image/png",
    };
}
