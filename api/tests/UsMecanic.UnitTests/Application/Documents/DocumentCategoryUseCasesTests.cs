using UsMecanic.Application.Documents.Categories;
using UsMecanic.Domain.Documents;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Documents;

public class DocumentCategoryUseCasesTests
{
    private readonly InMemoryDocumentCategoryRepository _documentCategories = new();

    [Fact]
    public async Task Cree_une_categorie_active()
    {
        var result = await new CreateDocumentCategory(_documentCategories).ExecuteAsync(" Nettoyage ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(_documentCategories.Saved);
    }

    [Fact]
    public async Task Refuse_un_nom_vide_ou_en_double_sans_tenir_compte_de_la_casse()
    {
        _documentCategories.Seed("Nettoyage");
        var useCase = new CreateDocumentCategory(_documentCategories);

        var empty = await useCase.ExecuteAsync("  ", CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync("NETTOYAGE", CancellationToken.None);

        Assert.Equal(DocumentCategoryErrors.NameRequired, empty.Error);
        Assert.Equal(DocumentCategoryErrors.NameAlreadyExists, duplicate.Error);
        Assert.Single(_documentCategories.Saved);
        Assert.Equal(0, _documentCategories.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_nom_trop_long()
    {
        var result = await new CreateDocumentCategory(_documentCategories).ExecuteAsync(new string('x', DocumentCategory.MaxNameLength + 1), CancellationToken.None);

        Assert.Equal(DocumentCategoryErrors.NameTooLong, result.Error);
        Assert.Empty(_documentCategories.Saved);
    }

    [Fact]
    public async Task Renomme_une_categorie_y_compris_pour_changer_la_casse()
    {
        var documentCategory = _documentCategories.Seed("nettoyage");

        var result = await new RenameDocumentCategory(_documentCategories).ExecuteAsync(documentCategory.Id, "Nettoyage", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", documentCategory.Name);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_existant_ou_une_categorie_inconnue()
    {
        _documentCategories.Seed("Nettoyage");
        var other = _documentCategories.Seed("Équipement");
        var useCase = new RenameDocumentCategory(_documentCategories);

        var duplicate = await useCase.ExecuteAsync(other.Id, "nettoyage", CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), "Divers", CancellationToken.None);
        var empty = await useCase.ExecuteAsync(other.Id, " ", CancellationToken.None);

        Assert.Equal(DocumentCategoryErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(DocumentCategoryErrors.NotFound, unknown.Error);
        Assert.Equal(DocumentCategoryErrors.NameRequired, empty.Error);
        Assert.Equal("Équipement", other.Name);
    }

    [Fact]
    public async Task Une_categorie_desactivee_n_est_plus_proposee_mais_reste_liste_sur_demande()
    {
        var documentCategory = _documentCategories.Seed("Nettoyage");

        var deactivated = await new DeactivateDocumentCategory(_documentCategories).ExecuteAsync(documentCategory.Id, CancellationToken.None);
        var proposed = await new ListDocumentCategories(_documentCategories).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListDocumentCategories(_documentCategories).ExecuteAsync(true, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Equal(documentCategory.Id, Assert.Single(all).Id);
    }

    [Fact]
    public async Task Desactiver_une_categorie_inconnue_renvoie_introuvable()
    {
        var result = await new DeactivateDocumentCategory(_documentCategories).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(DocumentCategoryErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task La_liste_est_triee_par_nom_sans_tenir_compte_de_la_casse()
    {
        _documentCategories.Seed("Divers");
        _documentCategories.Seed("abonnement");

        var list = await new ListDocumentCategories(_documentCategories).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("abonnement", list[0].Name);
        Assert.Equal("Divers", list[1].Name);
    }
}
