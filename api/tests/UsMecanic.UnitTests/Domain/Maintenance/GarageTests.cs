using UsMecanic.Domain.Maintenance;

namespace UsMecanic.UnitTests.Domain.Maintenance;

public class GarageTests
{
    [Fact]
    public void Create_nettoie_le_nom_et_cree_un_garage_actif()
    {
        var result = Garage.Create("  Garage Martin ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Garage Martin", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_un_nom_vide(string? name)
    {
        Assert.Equal(GarageErrors.NameRequired, Garage.Create(name).Error);
    }

    [Fact]
    public void Create_refuse_un_nom_trop_long()
    {
        var result = Garage.Create(new string('x', Garage.MaxNameLength + 1));

        Assert.Equal(GarageErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_accepte_un_nom_a_la_longueur_maximale()
    {
        Assert.True(Garage.Create(new string('x', Garage.MaxNameLength)).IsSuccess);
    }

    [Fact]
    public void Rename_change_le_nom_et_conserve_l_identifiant()
    {
        var garage = Garage.Create("Garage Martin").Value;
        var id = garage.Id;

        var result = garage.Rename(" Garage Durand ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Garage Durand", garage.Name);
        Assert.Equal(id, garage.Id);
    }

    [Fact]
    public void Rename_refuse_un_nom_vide_sans_modifier_le_garage()
    {
        var garage = Garage.Create("Garage Martin").Value;

        var result = garage.Rename(" ");

        Assert.Equal(GarageErrors.NameRequired, result.Error);
        Assert.Equal("Garage Martin", garage.Name);
    }

    [Fact]
    public void Deactivate_desactive_le_garage()
    {
        var garage = Garage.Create("Garage Martin").Value;

        garage.Deactivate();

        Assert.False(garage.IsActive);
    }
}
