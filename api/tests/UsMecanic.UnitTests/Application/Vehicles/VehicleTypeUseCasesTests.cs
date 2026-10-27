using UsMecanic.Application.Vehicles;
using UsMecanic.Application.Vehicles.Types;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class VehicleTypeUseCasesTests
{
    private readonly InMemoryVehicleTypeRepository _types = new();
    private readonly InMemoryVehicleRepository _vehicles = new();

    [Fact]
    public async Task Cree_un_type_actif()
    {
        var result = await new CreateVehicleType(_types).ExecuteAsync(" Ambulance ", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Ambulance", result.Value.Name);
        Assert.True(result.Value.IsActive);
        Assert.Single(_types.Saved);
    }

    [Fact]
    public async Task Refuse_un_nom_vide_ou_en_double_sans_tenir_compte_de_la_casse()
    {
        _types.Seed("Ambulance");
        var useCase = new CreateVehicleType(_types);

        var empty = await useCase.ExecuteAsync("  ", CancellationToken.None);
        var duplicate = await useCase.ExecuteAsync("ambulance", CancellationToken.None);

        Assert.Equal(VehicleTypeErrors.NameRequired, empty.Error);
        Assert.Equal(VehicleTypeErrors.NameAlreadyExists, duplicate.Error);
        Assert.Single(_types.Saved);
        Assert.Equal(0, _types.SaveCount);
    }

    [Fact]
    public async Task Renomme_un_type_y_compris_pour_changer_la_casse()
    {
        var type = _types.Seed("vsl");

        var result = await new RenameVehicleType(_types).ExecuteAsync(type.Id, "VSL", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("VSL", type.Name);
    }

    [Fact]
    public async Task Refuse_de_renommer_vers_un_nom_existant_ou_un_type_inconnu()
    {
        _types.Seed("Ambulance");
        var vsl = _types.Seed("VSL");
        var useCase = new RenameVehicleType(_types);

        var duplicate = await useCase.ExecuteAsync(vsl.Id, "Ambulance", CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(Guid.NewGuid(), "Sang", CancellationToken.None);

        Assert.Equal(VehicleTypeErrors.NameAlreadyExists, duplicate.Error);
        Assert.Equal(VehicleTypeErrors.NotFound, unknown.Error);
        Assert.Equal("VSL", vsl.Name);
    }

    [Fact]
    public async Task Un_type_desactive_n_est_plus_propose_mais_reste_affiche_sur_le_vehicule()
    {
        var type = _types.Seed("Sang");
        var vehicle = _vehicles.Seed("AB-123-CD", new VehicleDetails(null, null, null, null, null, type.Id));

        var deactivated = await new DeactivateVehicleType(_types).ExecuteAsync(type.Id, CancellationToken.None);
        var proposed = await new ListVehicleTypes(_types).ExecuteAsync(false, CancellationToken.None);
        var all = await new ListVehicleTypes(_types).ExecuteAsync(true, CancellationToken.None);
        var dto = await new GetVehicle(_vehicles, new VehicleTypeLookup(_types, new InMemoryMileageReadingRepository())).ExecuteAsync(vehicle.Id, CancellationToken.None);

        Assert.False(deactivated.Value.IsActive);
        Assert.Empty(proposed);
        Assert.Single(all);
        Assert.Equal("Sang", dto.Value.TypeName);
    }

    [Fact]
    public async Task La_liste_est_triee_par_nom()
    {
        _types.Seed("VSL");
        _types.Seed("ambulance");

        var list = await new ListVehicleTypes(_types).ExecuteAsync(false, CancellationToken.None);

        Assert.Equal("ambulance", list[0].Name);
        Assert.Equal("VSL", list[1].Name);
    }

    [Fact]
    public async Task Desactiver_un_type_inconnu_renvoie_introuvable()
    {
        var result = await new DeactivateVehicleType(_types).ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(VehicleTypeErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Un_vehicule_conserve_son_type_desactive_mais_ne_peut_pas_en_recevoir_un()
    {
        var oldType = _types.Seed("Ancien", active: false);
        var vehicle = _vehicles.Seed("AB-123-CD", new VehicleDetails(null, null, null, null, null, oldType.Id));
        var useCase = new UpdateVehicleDetails(_vehicles, new VehicleTypeLookup(_types, new InMemoryMileageReadingRepository()), new FixedClock(), new FixedClock().Calendar);

        var kept = await useCase.ExecuteAsync(vehicle.Id, new VehicleDetailsInput("Renault", null, null, null, null, oldType.Id), CancellationToken.None);
        var other = _types.Seed("Autre", active: false);
        var refused = await useCase.ExecuteAsync(vehicle.Id, new VehicleDetailsInput("Renault", null, null, null, null, other.Id), CancellationToken.None);

        Assert.True(kept.IsSuccess);
        Assert.Equal(VehicleTypeErrors.Inactive, refused.Error);
        Assert.Equal(oldType.Id, vehicle.TypeId);
    }

    [Fact]
    public async Task Le_filtre_par_type_ne_garde_que_les_vehicules_du_type()
    {
        var type = _types.Seed("Ambulance");
        var typed = _vehicles.Seed("AB-123-CD", new VehicleDetails(null, null, null, null, null, type.Id));
        _vehicles.Seed("EF-456-GH");

        var list = await new ListVehicles(_vehicles, new VehicleTypeLookup(_types, new InMemoryMileageReadingRepository()))
            .ExecuteAsync(new VehicleFilter(TypeId: type.Id), CancellationToken.None);

        var found = Assert.Single(list);
        Assert.Equal(typed.Id, found.Id);
        Assert.Equal("Ambulance", found.TypeName);
    }
}
