using UsMecanic.Domain.Companies;

namespace UsMecanic.UnitTests.Domain.Companies;

public class CompanyTests
{
    [Fact]
    public void Create_nettoie_le_nom_et_cree_une_societe_active()
    {
        var result = Company.Create("  Nettoyage ");

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
        Assert.Equal(CompanyErrors.NameRequired, Company.Create(name).Error);
    }

    [Fact]
    public void Create_refuse_un_nom_trop_long()
    {
        var result = Company.Create(new string('x', Company.MaxNameLength + 1));

        Assert.Equal(CompanyErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_accepte_un_nom_a_la_longueur_maximale()
    {
        Assert.True(Company.Create(new string('x', Company.MaxNameLength)).IsSuccess);
    }

    [Fact]
    public void Rename_change_le_nom_et_conserve_l_identifiant()
    {
        var company = Company.Create("Nettoyage").Value;
        var id = company.Id;

        var result = company.Rename(" Équipement ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Équipement", company.Name);
        Assert.Equal(id, company.Id);
    }

    [Fact]
    public void Rename_refuse_un_nom_vide_sans_modifier_la_societe()
    {
        var company = Company.Create("Nettoyage").Value;

        var result = company.Rename(" ");

        Assert.Equal(CompanyErrors.NameRequired, result.Error);
        Assert.Equal("Nettoyage", company.Name);
    }

    [Fact]
    public void Deactivate_desactive_la_societe()
    {
        var company = Company.Create("Nettoyage").Value;

        company.Deactivate();

        Assert.False(company.IsActive);
    }

    [Fact]
    public void GetSelectionError_accepte_une_societe_active()
    {
        var company = Company.Create("Nettoyage").Value;

        Assert.Null(company.GetSelectionError());
    }

    [Fact]
    public void GetSelectionError_refuse_une_societe_inactive()
    {
        var company = Company.Create("Nettoyage").Value;
        company.Deactivate();

        Assert.Equal(CompanyErrors.Inactive, company.GetSelectionError());
    }
}
