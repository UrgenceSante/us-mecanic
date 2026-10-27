using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class RegisterVehicleTests
{
    private readonly InMemoryVehicleRepository _repository = new();
    private readonly InMemoryVehicleTypeRepository _types = new();
    private readonly RegisterVehicle _useCase;

    public RegisterVehicleTests() => _useCase = new RegisterVehicle(_repository, new VehicleTypeLookup(_types), new FixedClock());

    private static RegisterVehicleCommand Command(string plate, string? vin = null, Guid? typeId = null) =>
        new(plate, new VehicleDetailsInput("Renault", "Master", null, vin, null, typeId));

    [Fact]
    public async Task Enregistre_le_vehicule_avec_son_type_actif()
    {
        var type = _types.Seed("Ambulance");

        var result = await _useCase.ExecuteAsync(Command("AB-123-CD", typeId: type.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(type.Id, result.Value.TypeId);
        Assert.Equal("Ambulance", result.Value.TypeName);
    }

    [Fact]
    public async Task Refuse_un_type_inconnu_ou_desactive_sans_rien_enregistrer()
    {
        var inactive = _types.Seed("VSL", active: false);

        var unknown = await _useCase.ExecuteAsync(Command("AB-123-CD", typeId: Guid.NewGuid()), CancellationToken.None);
        var deactivated = await _useCase.ExecuteAsync(Command("AB-123-CD", typeId: inactive.Id), CancellationToken.None);

        Assert.Equal(VehicleTypeErrors.Unknown, unknown.Error);
        Assert.Equal(VehicleTypeErrors.Inactive, deactivated.Error);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Enregistre_le_vehicule_avec_une_plaque_normalisee()
    {
        var result = await _useCase.ExecuteAsync(Command("ab123cd", "VF1MA000012345678"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("AB-123-CD", result.Value.LicensePlate);
        Assert.Equal("VF1MA000012345678", result.Value.Vin);
        Assert.Single(_repository.Saved);
    }

    [Fact]
    public async Task Refuse_une_immatriculation_deja_enregistree()
    {
        _repository.Seed("AB-123-CD");

        var result = await _useCase.ExecuteAsync(Command("AB 123 CD"), CancellationToken.None);

        Assert.Equal(VehicleErrors.PlateAlreadyExists, result.Error);
        Assert.Single(_repository.Saved);
    }

    [Theory]
    [InlineData("pas-une-plaque", null)]
    [InlineData("AB-123-CD", "VIN-INVALIDE")]
    public async Task Refuse_les_saisies_invalides_sans_rien_enregistrer(string plate, string? vin)
    {
        var result = await _useCase.ExecuteAsync(Command(plate, vin), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(0, _repository.SaveCount);
    }
}
