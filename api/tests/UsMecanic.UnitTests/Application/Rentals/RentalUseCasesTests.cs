using UsMecanic.Application.Rentals;
using UsMecanic.Domain.Companies;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Rentals;

public class RentalUseCasesTests
{
    private static readonly DateOnly Feb1 = new(2026, 2, 1);
    private static readonly DateOnly Mar31 = new(2026, 3, 31);

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryCompanyRepository _companies = new();
    private readonly InMemoryRentalRepository _rentals = new();
    private readonly Company _owner;
    private readonly Company _tenant;
    private readonly Vehicle _vehicle;

    public RentalUseCasesTests()
    {
        _owner = _companies.Seed("Ambulances Nord");
        _tenant = _companies.Seed("Pompes Sud");
        _vehicle = _vehicles.Seed("AB-123-CD", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = _owner.Id });
    }

    private RentalDtoFactory Factory() => new(_companies, new FixedClock().Calendar);

    private RentalWriteChecker Checker() => new(_companies, _rentals);

    private AddRental Add() => new(_vehicles, _rentals, Checker(), Factory());

    private UpdateRental Update() => new(_vehicles, _rentals, Checker(), Factory());

    private EndRental End() => new(_vehicles, _rentals, Checker(), Factory());

    private DeleteRental Delete() => new(_vehicles, _rentals, Factory());

    private ListRentals List() => new(_vehicles, _rentals, Factory());

    private RentalInput Input(DateOnly start, DateOnly? end, Guid? tenant = null) => new(tenant ?? _tenant.Id, start, end);

    [Fact]
    public async Task Cree_une_location_avec_son_etat()
    {
        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, Mar31), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_tenant.Id, result.Value.Tenant.Id);
        Assert.Equal(RentalState.Ended, result.Value.State);
        Assert.Single(_rentals.Saved);
    }

    [Fact]
    public async Task Accepte_une_location_prevue_et_une_location_d_un_jour()
    {
        var planned = await Add().ExecuteAsync(_vehicle.Id, Input(FixedClock.DefaultToday.AddDays(10), null), CancellationToken.None);
        var oneDay = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, Feb1), CancellationToken.None);

        Assert.Equal(RentalState.Planned, planned.Value.State);
        Assert.True(oneDay.IsSuccess);
    }

    [Fact]
    public async Task Refuse_un_vehicule_sans_proprietaire()
    {
        var vehicle = _vehicles.Seed("EF-456-GH");

        var result = await Add().ExecuteAsync(vehicle.Id, Input(Feb1, null), CancellationToken.None);

        Assert.Equal(RentalErrors.OwnerRequired, result.Error);
    }

    [Fact]
    public async Task Refuse_le_proprietaire_comme_locataire()
    {
        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, null, _owner.Id), CancellationToken.None);

        Assert.Equal(RentalErrors.TenantIsOwner, result.Error);
    }

    [Fact]
    public async Task Refuse_une_fin_anterieure_au_debut()
    {
        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Mar31, Feb1), CancellationToken.None);

        Assert.Equal(RentalErrors.EndBeforeStart, result.Error);
    }

    [Fact]
    public async Task Refuse_un_locataire_inactif_ou_inconnu_a_la_creation()
    {
        var inactive = _companies.Seed("Ancienne", active: false);

        var refused = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, null, inactive.Id), CancellationToken.None);
        var unknown = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, null, Guid.NewGuid()), CancellationToken.None);
        var missing = await Add().ExecuteAsync(_vehicle.Id, new RentalInput(null, Feb1, null), CancellationToken.None);

        Assert.Equal(CompanyErrors.Inactive, refused.Error);
        Assert.Equal(CompanyErrors.Unknown, unknown.Error);
        Assert.Equal(RentalErrors.TenantRequired, missing.Error);
    }

    [Fact]
    public async Task Refuse_un_vehicule_sorti_du_parc()
    {
        _vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, null), CancellationToken.None);

        Assert.Equal(RentalErrors.VehicleRetired, result.Error);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu()
    {
        var result = await Add().ExecuteAsync(Guid.NewGuid(), Input(Feb1, null), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Refuse_un_chevauchement_en_nommant_la_location_en_conflit()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, Mar31);

        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Mar31, null), CancellationToken.None);

        Assert.Equal("rental.overlap", result.Error?.Code);
        Assert.Equal("Chevauche la location à Pompes Sud du 01/02/2026 au 31/03/2026", result.Error?.Message);
    }

    [Fact]
    public async Task Refuse_une_location_sans_fin_qui_commence_avant_une_location_future()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, FixedClock.DefaultToday.AddDays(30), Mar31.AddYears(1));

        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, null), CancellationToken.None);

        Assert.Equal("rental.overlap", result.Error?.Code);
        Assert.Single(_rentals.Saved);
    }

    [Fact]
    public async Task Chevauchement_avec_une_location_sans_fin()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, null);

        var result = await Add().ExecuteAsync(_vehicle.Id, Input(new DateOnly(2030, 1, 1), null), CancellationToken.None);

        Assert.Equal("Chevauche la location à Pompes Sud depuis le 01/02/2026, sans date de fin", result.Error?.Message);
    }

    [Fact]
    public async Task Accepte_une_location_qui_commence_le_lendemain_de_la_precedente()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, Mar31);

        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Mar31.AddDays(1), null), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Les_locations_des_autres_vehicules_ne_genent_pas()
    {
        var other = _vehicles.Seed("EF-456-GH", InMemoryVehicleRepository.EmptyDetails with { OwnerCompanyId = _owner.Id });
        _rentals.Seed(other.Id, _tenant.Id, Feb1, null);

        var result = await Add().ExecuteAsync(_vehicle.Id, Input(Feb1, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Modification_ignore_la_location_modifiee_dans_le_controle_de_chevauchement()
    {
        var rental = _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, Mar31);

        var result = await Update().ExecuteAsync(_vehicle.Id, rental.Id, Input(Feb1, Mar31.AddDays(10)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Mar31.AddDays(10), rental.EndDate);
    }

    [Fact]
    public async Task Modification_refuse_un_chevauchement()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, Mar31);
        var second = _rentals.Seed(_vehicle.Id, _tenant.Id, Mar31.AddDays(1), null);

        var result = await Update().ExecuteAsync(_vehicle.Id, second.Id, Input(Mar31, null), CancellationToken.None);

        Assert.Equal("rental.overlap", result.Error?.Code);
        Assert.Equal(Mar31.AddDays(1), second.StartDate);
    }

    [Fact]
    public async Task Modification_garde_un_locataire_desactive_mais_refuse_un_nouveau_locataire_inactif()
    {
        var rental = _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, null);
        var inactive = _companies.Seed("Ancienne", active: false);
        _tenant.Deactivate();

        var kept = await Update().ExecuteAsync(_vehicle.Id, rental.Id, Input(Feb1, Mar31), CancellationToken.None);
        var changed = await Update().ExecuteAsync(_vehicle.Id, rental.Id, Input(Feb1, Mar31, inactive.Id), CancellationToken.None);

        Assert.True(kept.IsSuccess);
        Assert.False(kept.Value.Tenant.IsActive);
        Assert.Equal(CompanyErrors.Inactive, changed.Error);
    }

    [Fact]
    public async Task Modification_possible_sur_un_vehicule_sorti_du_parc()
    {
        var rental = _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, null);
        _vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var result = await Update().ExecuteAsync(_vehicle.Id, rental.Id, Input(Feb1, Mar31), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Modification_d_une_location_inconnue()
    {
        var result = await Update().ExecuteAsync(_vehicle.Id, Guid.NewGuid(), Input(Feb1, null), CancellationToken.None);

        Assert.Equal(RentalErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Termine_une_location_sans_fin_meme_hors_parc()
    {
        var rental = _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, null);
        _vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var result = await End().ExecuteAsync(_vehicle.Id, rental.Id, Mar31, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(Mar31, rental.EndDate);
    }

    [Fact]
    public async Task Terminer_refuse_une_fin_anterieure_au_debut_et_une_location_deja_terminee()
    {
        var open = _rentals.Seed(_vehicle.Id, _tenant.Id, Mar31, null);
        var closed = _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, Mar31.AddDays(-1));

        var before = await End().ExecuteAsync(_vehicle.Id, open.Id, Feb1, CancellationToken.None);
        var again = await End().ExecuteAsync(_vehicle.Id, closed.Id, Mar31, CancellationToken.None);

        Assert.Equal(RentalErrors.EndBeforeStart, before.Error);
        Assert.Null(open.EndDate);
        Assert.Equal(RentalErrors.AlreadyEnded, again.Error);
    }

    [Fact]
    public async Task Supprime_une_location_meme_hors_parc()
    {
        var rental = _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, null);
        _vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var result = await Delete().ExecuteAsync(_vehicle.Id, rental.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_tenant.Id, result.Value.Tenant.Id);
        Assert.Empty(_rentals.Saved);
    }

    [Fact]
    public async Task Suppression_d_une_location_inconnue()
    {
        var result = await Delete().ExecuteAsync(_vehicle.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(RentalErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Liste_de_la_plus_recente_a_la_plus_ancienne_avec_etat()
    {
        _rentals.Seed(_vehicle.Id, _tenant.Id, Feb1, Mar31);
        _rentals.Seed(_vehicle.Id, _tenant.Id, FixedClock.DefaultToday.AddDays(-5), null);

        var result = await List().ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal([RentalState.Ongoing, RentalState.Ended], result.Value.Select(r => r.State));
    }

    [Fact]
    public async Task Liste_d_un_vehicule_inconnu()
    {
        var result = await List().ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
    }
}
