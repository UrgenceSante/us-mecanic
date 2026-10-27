using UsMecanic.Domain.Vehicles;

namespace UsMecanic.UnitTests.Domain.Vehicles;

public class VehicleTypeTests
{
    [Fact]
    public void Create_nettoie_le_nom_et_cree_un_type_actif()
    {
        var result = VehicleType.Create("  Ambulance ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ambulance", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_refuse_un_nom_vide(string? name)
    {
        Assert.Equal(VehicleTypeErrors.NameRequired, VehicleType.Create(name).Error);
    }

    [Fact]
    public void Create_refuse_un_nom_trop_long()
    {
        var result = VehicleType.Create(new string('x', VehicleType.MaxNameLength + 1));

        Assert.Equal(VehicleTypeErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Rename_change_le_nom_et_conserve_l_identifiant()
    {
        var vehicleType = VehicleType.Create("VSL").Value;
        var id = vehicleType.Id;

        var result = vehicleType.Rename(" Véhicule sanitaire léger ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Véhicule sanitaire léger", vehicleType.Name);
        Assert.Equal(id, vehicleType.Id);
    }

    [Fact]
    public void Rename_refuse_un_nom_vide_sans_modifier_le_type()
    {
        var vehicleType = VehicleType.Create("VSL").Value;

        var result = vehicleType.Rename(" ");

        Assert.Equal(VehicleTypeErrors.NameRequired, result.Error);
        Assert.Equal("VSL", vehicleType.Name);
    }

    [Fact]
    public void Deactivate_desactive_le_type()
    {
        var vehicleType = VehicleType.Create("Sang").Value;

        vehicleType.Deactivate();

        Assert.False(vehicleType.IsActive);
    }
}
