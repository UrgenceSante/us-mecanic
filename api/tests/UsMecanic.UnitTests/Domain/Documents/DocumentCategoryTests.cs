using UsMecanic.Domain.Documents;

namespace UsMecanic.UnitTests.Domain.Documents;

public class DocumentCategoryTests
{
    [Fact]
    public void Create_nettoie_le_nom_et_cree_une_categorie_active()
    {
        var result = DocumentCategory.Create("  Nettoyage ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_un_nom_vide(string? name)
    {
        Assert.Equal(DocumentCategoryErrors.NameRequired, DocumentCategory.Create(name).Error);
    }

    [Fact]
    public void Create_refuse_un_nom_trop_long()
    {
        var result = DocumentCategory.Create(new string('x', DocumentCategory.MaxNameLength + 1));

        Assert.Equal(DocumentCategoryErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_accepte_un_nom_a_la_longueur_maximale()
    {
        Assert.True(DocumentCategory.Create(new string('x', DocumentCategory.MaxNameLength)).IsSuccess);
    }

    [Fact]
    public void Rename_change_le_nom_et_conserve_l_identifiant()
    {
        var documentCategory = DocumentCategory.Create("Nettoyage").Value;
        var id = documentCategory.Id;

        var result = documentCategory.Rename(" Équipement ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Équipement", documentCategory.Name);
        Assert.Equal(id, documentCategory.Id);
    }

    [Fact]
    public void Rename_refuse_un_nom_vide_sans_modifier_la_categorie()
    {
        var documentCategory = DocumentCategory.Create("Nettoyage").Value;

        var result = documentCategory.Rename(" ");

        Assert.Equal(DocumentCategoryErrors.NameRequired, result.Error);
        Assert.Equal("Nettoyage", documentCategory.Name);
    }

    [Fact]
    public void Deactivate_desactive_la_categorie()
    {
        var documentCategory = DocumentCategory.Create("Nettoyage").Value;

        documentCategory.Deactivate();

        Assert.False(documentCategory.IsActive);
    }

    [Fact]
    public void GetSelectionError_accepte_une_categorie_active()
    {
        var category = DocumentCategory.Create("Nettoyage").Value;

        Assert.Null(category.GetSelectionError());
    }

    [Fact]
    public void GetSelectionError_refuse_une_categorie_inactive()
    {
        var category = DocumentCategory.Create("Nettoyage").Value;
        category.Deactivate();

        Assert.Equal(DocumentCategoryErrors.Inactive, category.GetSelectionError());
    }
}
