using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class VehicleOwnerUseCasesTests
{
    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryVehicleTypeRepository _types = new();
    private readonly InMemoryCompanyRepository _companies = new();
    private readonly VehicleOwnerLookup _owners;
    private readonly VehicleTypeLookup _lookup;

    public VehicleOwnerUseCasesTests()
    {
        _owners = new VehicleOwnerLookup(_companies, new InMemoryRentalRepository(), new FixedClock().Calendar);
        _lookup = new VehicleTypeLookup(_types, new InMemoryMileageReadingRepository(), _owners);
    }

    private RegisterVehicle Register() => new(_vehicles, _lookup, _owners, new FixedClock(), new FixedClock().Calendar);

    private UpdateVehicleDetails Update() => new(_vehicles, _lookup, _owners, new FixedClock(), new FixedClock().Calendar);

    private static VehicleDetailsInput Input(Guid? owner) => new("Renault", null, null, null, null, null, owner);

    [Fact]
    public async Task Enregistre_un_vehicule_avec_proprietaire_actif_et_expose_utilisateur()
    {
        var company = _companies.Seed("Ambulances Nord");

        var result = await Register().ExecuteAsync(new RegisterVehicleCommand("AB-123-CD", Input(company.Id)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id, result.Value.Owner?.Id);
        Assert.Equal(VehicleUserSource.Owner, result.Value.CurrentUser.Source);
        Assert.Equal(company.Id, result.Value.CurrentUser.Company?.Id);
    }

    [Fact]
    public async Task Refuse_une_requete_sans_bloc_details()
    {
        var result = await Register().ExecuteAsync(new RegisterVehicleCommand("AB-123-CD", null), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("request.invalid", result.Error!.Code);
    }

    [Fact]
    public async Task Sans_proprietaire_utilisateur_non_renseigne()
    {
        var result = await Register().ExecuteAsync(new RegisterVehicleCommand("AB-123-CD", Input(null)), CancellationToken.None);

        Assert.Null(result.Value.Owner);
        Assert.Equal(VehicleUserSource.NotSet, result.Value.CurrentUser.Source);
        Assert.Null(result.Value.CurrentUser.Company);
    }

    [Fact]
    public async Task Refuse_une_societe_inactive_ou_inconnue()
    {
        var inactive = _companies.Seed("Ancienne", active: false);

        var refused = await Register().ExecuteAsync(new RegisterVehicleCommand("AB-123-CD", Input(inactive.Id)), CancellationToken.None);
        var unknown = await Register().ExecuteAsync(new RegisterVehicleCommand("AB-123-CD", Input(Guid.NewGuid())), CancellationToken.None);

        Assert.Equal(CompanyErrors.Inactive, refused.Error);
        Assert.Equal(CompanyErrors.Unknown, unknown.Error);
        Assert.Empty(_vehicles.Saved);
    }

    [Fact]
    public async Task Modification_refuse_de_vider_le_proprietaire()
    {
        var company = _companies.Seed("Ambulances Nord");
        var vehicle = _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = company.Id });

        var result = await Update().ExecuteAsync(vehicle.Id, Input(null), CancellationToken.None);

        Assert.Equal(VehicleErrors.OwnerCannotBeRemoved, result.Error);
    }

    [Fact]
    public async Task Modification_conserve_un_proprietaire_desactive_et_l_affiche()
    {
        var company = _companies.Seed("Ancienne");
        var vehicle = _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = company.Id });
        company.Deactivate();

        var result = await Update().ExecuteAsync(vehicle.Id, Input(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.Owner?.IsActive);
    }

    [Fact]
    public async Task Modification_refuse_de_changer_pour_une_societe_inactive()
    {
        var current = _companies.Seed("Actuelle");
        var inactive = _companies.Seed("Ancienne", active: false);
        var vehicle = _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = current.Id });

        var result = await Update().ExecuteAsync(vehicle.Id, Input(inactive.Id), CancellationToken.None);

        Assert.Equal(CompanyErrors.Inactive, result.Error);
    }

    [Fact]
    public async Task Vehicule_sorti_du_parc_sans_proprietaire_accepte_la_saisie_du_proprietaire()
    {
        var company = _companies.Seed("Ambulances Nord");
        var vehicle = _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { Brand = "Renault" });
        vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var result = await Update().ExecuteAsync(vehicle.Id, Input(company.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(company.Id, result.Value.Owner?.Id);
    }

    [Fact]
    public async Task Liste_expose_le_proprietaire_de_chaque_vehicule()
    {
        var company = _companies.Seed("Ambulances Nord");
        _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = company.Id });
        _vehicles.Seed("EF-456-GH");

        var list = await new ListVehicles(_vehicles, _lookup, NoAlerts.Lookup()).ExecuteAsync(new VehicleFilter(), CancellationToken.None);

        Assert.Equal(company.Id, list.Single(v => v.LicensePlate == "AB-123-CD").Owner?.Id);
        Assert.Equal(VehicleUserSource.NotSet, list.Single(v => v.LicensePlate == "EF-456-GH").CurrentUser.Source);
    }
}
