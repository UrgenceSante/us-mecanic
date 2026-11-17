using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents.Categories;

public sealed record DocumentCategoryDto(Guid Id, string Name, bool IsActive)
{
    public static DocumentCategoryDto From(DocumentCategory documentCategory)
    {
        ArgumentNullException.ThrowIfNull(documentCategory);

        return new(documentCategory.Id, documentCategory.Name, documentCategory.IsActive);
    }
}
