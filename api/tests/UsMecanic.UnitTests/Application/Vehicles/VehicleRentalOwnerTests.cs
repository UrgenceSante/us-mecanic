using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

/// <summary>Propriétaire et utilisateur actuel d'un véhicule en présence de locations (US2, US4, R4).</summary>
public class VehicleRentalOwnerTests
{
    private static readonly DateOnly Today = FixedClock.DefaultToday;

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryCompanyRepository _companies = new();
    private readonly InMemoryRentalRepository _rentals = new();
    private readonly VehicleOwnerLookup _owners;
    private readonly VehicleTypeLookup _lookup;
    private readonly Company _owner;
    private readonly Company _tenant;
    private readonly Vehicle _vehicle;

    public VehicleRentalOwnerTests()
    {
        _owners = new VehicleOwnerLookup(_companies, _rentals, new FixedClock().Calendar);
        _lookup = new VehicleTypeLookup(new InMemoryVehicleTypeRepository(), new InMemoryMileageReadingRepository(), _owners);
        _owner = _companies.Seed("Ambulances Nord");
        _tenant = _companies.Seed("Pompes Sud");
        _vehicle = _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = _owner.Id });
    }

    private UpdateVehicleDetails Update() => new(_vehicles, _lookup, _owners, new FixedClock(), new FixedClock().Calendar);

    private static VehicleDetailsInput Input(Guid? owner) => new("Renault", null, null, null, null, null, owner);

    [Fact]
    public async Task Fiche_et_liste_exposent_le_locataire_en_cours_comme_utilisateur()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Today.AddDays(-3), null);
        _vehicles.Seed("EF-456-GH", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = _owner.Id });

        var dto = await new GetVehicle(_vehicles, _lookup).ExecuteAsync(_vehicle.Id, CancellationToken.None);
        var list = await new ListVehicles(_vehicles, _lookup, NoAlerts.Lookup()).ExecuteAsync(new VehicleFilter(), CancellationToken.None);

        Assert.Equal(VehicleUserSource.Tenant, dto.Value.CurrentUser.Source);
        Assert.Equal(_tenant.Id, dto.Value.CurrentUser.Company?.Id);
        Assert.Equal(_owner.Id, dto.Value.Owner?.Id);
        Assert.Equal([VehicleUserSource.Tenant, VehicleUserSource.Owner], list.Select(v => v.CurrentUser.Source));
    }

    [Fact]
    public async Task Filtre_la_liste_par_proprietaire_sans_proprietaire_et_utilisateur_actuel()
    {
        var orphan = _vehicles.Seed("EF-456-GH");
        _rentals.Seed(_vehicle.Id, _tenant.Id, Today.AddDays(-3), null);
        var list = new ListVehicles(_vehicles, _lookup, NoAlerts.Lookup());

        var byOwner = await list.ExecuteAsync(new VehicleFilter(OwnerCompanyId: _owner.Id), CancellationToken.None);
        var missing = await list.ExecuteAsync(new VehicleFilter(OwnerMissing: true), CancellationToken.None);
        var byTenant = await list.ExecuteAsync(new VehicleFilter(CurrentUserCompanyId: _tenant.Id), CancellationToken.None);
        var byOwnerUser = await list.ExecuteAsync(new VehicleFilter(CurrentUserCompanyId: _owner.Id), CancellationToken.None);
        var combined = await list.ExecuteAsync(new VehicleFilter(OwnerCompanyId: _owner.Id, CurrentUserCompanyId: _tenant.Id), CancellationToken.None);

        Assert.Equal([_vehicle.Id], byOwner.Select(v => v.Id));
        Assert.Equal([orphan.Id], missing.Select(v => v.Id));
        Assert.Equal([_vehicle.Id], byTenant.Select(v => v.Id));
        Assert.Empty(byOwnerUser);
        Assert.Equal([_vehicle.Id], combined.Select(v => v.Id));
    }

    [Fact]
    public async Task Une_location_terminee_ou_prevue_laisse_le_proprietaire_comme_utilisateur()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Today.AddDays(-30), Today.AddDays(-1));
        _rentals.Seed(_vehicle.Id, _tenant.Id, Today.AddDays(1), null);

        var dto = await new GetVehicle(_vehicles, _lookup).ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal(VehicleUserSource.Owner, dto.Value.CurrentUser.Source);
    }

    [Fact]
    public async Task Refuse_un_nouveau_proprietaire_qui_loue_le_vehicule_en_cours_ou_a_venir()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Today.AddDays(10), Today.AddDays(20));

        var result = await Update().ExecuteAsync(_vehicle.Id, Input(_tenant.Id), CancellationToken.None);

        Assert.Equal("Pompes Sud loue ce véhicule : terminez ou supprimez d'abord cette location.", result.Error?.Message);
        Assert.Equal(_owner.Id, _vehicle.OwnerCompanyId);
    }

    [Fact]
    public async Task Accepte_un_nouveau_proprietaire_dont_la_location_est_terminee_ou_sur_un_autre_vehicule()
    {
        var other = _vehicles.Seed("EF-456-GH", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = _owner.Id });
        _rentals.Seed(other.Id, _tenant.Id, Today, null);
        _rentals.Seed(_vehicle.Id, _tenant.Id, Today.AddDays(-30), Today.AddDays(-1));

        var result = await Update().ExecuteAsync(_vehicle.Id, Input(_tenant.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_tenant.Id, result.Value.Owner?.Id);
    }
}
