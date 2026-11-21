using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Documents;

/// <summary>Erreurs métier des documents. Aucun message ne reprend le nom ni le contenu d'un fichier.</summary>
public static class DocumentErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("document.not_found", "Document introuvable.");

    public static readonly DomainError CategoryRequired =
        new("document.category_required", "La catégorie est obligatoire pour un document du véhicule.");

    public static readonly DomainError AttachmentRequired =
        new("document.attachment_required", "Une facture doit être rattachée à une intervention ou à une dépense.");

    public static readonly DomainError AttachmentAmbiguous =
        new("document.attachment_ambiguous", "Une facture est rattachée à une intervention ou à une dépense, pas aux deux.");

    public static readonly DomainError LabelTooLong =
        new("document.label_too_long", $"Le libellé est limité à {Document.MaxLabelLength} caractères.");

    public static readonly DomainError FileEmpty =
        new("document.file_empty", "Le fichier est vide.");

    public static readonly DomainError FileTooLarge =
        new("document.file_too_large", $"Le fichier dépasse la taille maximale de {Document.MaxSizeBytes / (1024 * 1024)} Mo.");

    public static readonly DomainError FileTypeUnsupported =
        new("document.file_type_unsupported", "Format de fichier non accepté : seuls les PDF, JPEG et PNG sont acceptés.");

    public static readonly DomainError FileNameRequired =
        new("document.file_name_required", "Le nom du fichier est obligatoire.");
}
