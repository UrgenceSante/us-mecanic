using UsMecanic.Application.Companies;
using UsMecanic.Domain.Companies;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Companies;

public class CompanyUseCasesTests
{
    private readonly InMemoryCompanyRepository _companies = new();

    [Fact]
    public async Task Cree_une_categorie_active()
    {
        var result = await new CreateCompany(_companies).ExecuteAsync(" Nettoyage ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(_companies.Saved);
    }

    [Fact]
    public async Task Refuse_un_nom_vide_ou_en_double_sans_tenir_compte_de_la_casse()
    {
        _companies.Seed("Nettoyage");
        var useCase = new CreateCompany(_companies);

        var empty = await useCase.ExecuteAsync("  ", CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync("NETTOYAGE", CancellationToken.None);

        Assert.Equal(CompanyErrors.NameRequired, empty.Error);
        Assert.Equal(CompanyErrors.NameAlreadyExists, duplicate.Error);
        Assert.Single(_companies.Saved);
        Assert.Equal(0, _companies.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_nom_trop_long()
    {
        var result = await new CreateCompany(_companies).ExecuteAsync(new string('x', Company.MaxNameLength + 1), CancellationToken.None);

        Assert.Equal(CompanyErrors.NameTooLong, result.Error);
        Assert.Empty(_companies.Saved);
    }

    [Fact]
    public async Task Renomme_une_categorie_y_compris_pour_changer_la_casse()
    {
        var company = _companies.Seed("nettoyage");

        var result = await new RenameCompany(_companies).ExecuteAsync(company.Id, "Nettoyage", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Nettoyage", company.Name);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_existant_ou_une_categorie_inconnue()
    {
        _companies.Seed("Nettoyage");
        var other = _companies.Seed("Équipement");
        var useCase = new RenameCompany(_companies);

        var duplicate = await useCase.ExecuteAsync(other.Id, "nettoyage", CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), "Divers", CancellationToken.None);
        var empty = await useCase.ExecuteAsync(other.Id, " ", CancellationToken.None);

        Assert.Equal(CompanyErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(CompanyErrors.NotFound, unknown.Error);
        Assert.Equal(CompanyErrors.NameRequired, empty.Error);
        Assert.Equal("Équipement", other.Name);
    }

    [Fact]
    public async Task Une_categorie_desactivee_n_est_plus_proposee_mais_reste_liste_sur_demande()
    {
        var company = _companies.Seed("Nettoyage");

        var deactivated = await new DeactivateCompany(_companies).ExecuteAsync(company.Id, CancellationToken.None);
        var proposed = await new ListCompanies(_companies).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListCompanies(_companies).ExecuteAsync(true, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Equal(company.Id, Assert.Single(all).Id);
    }

    [Fact]
    public async Task Desactiver_une_categorie_inconnue_renvoie_introuvable()
    {
        var result = await new DeactivateCompany(_companies).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(CompanyErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task La_liste_est_triee_par_nom_sans_tenir_compte_de_la_casse()
    {
        _companies.Seed("Divers");
        _companies.Seed("abonnement");

        var list = await new ListCompanies(_companies).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("abonnement", list[0].Name);
        Assert.Equal("Divers", list[1].Name);
    }
}
