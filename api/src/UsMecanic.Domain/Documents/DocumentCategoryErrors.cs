using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Documents;

public static class DocumentCategoryErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("document_category.not_found", "Catégorie de document introuvable.");

    public static readonly DomainError NameRequired =
        new("document_category.name_required", "Le nom de la catégorie est obligatoire.");

    public static readonly DomainError NameTooLong =
        new("document_category.name_too_long", $"Le nom de la catégorie est limité à {DocumentCategory.MaxNameLength} caractères.");

    public static readonly DomainError NameAlreadyExists =
        DomainError.Conflict("document_category.name_already_exists", "Une catégorie porte déjà ce nom.");

    public static readonly DomainError Unknown =
        new("document_category.unknown", "Catégorie de document inconnue.");

    public static readonly DomainError Inactive =
        new("document_category.inactive", "Cette catégorie est désactivée : elle ne peut plus être choisie pour une nouveau document.");
}
