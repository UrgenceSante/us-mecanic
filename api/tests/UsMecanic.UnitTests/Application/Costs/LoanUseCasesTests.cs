using UsMecanic.Application.Costs.Loans;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Costs;

public class LoanUseCasesTests
{
    private static readonly LoanInput Valid = new("Banque Fictive", 15_000m, 280m, new DateOnly(2026, 6, 1), 48);

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryLoanRepository _loans = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;

    public LoanUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
    }

    private MileageVehicleGuard Guard => new(_vehicles);

    private AddLoan Add => new(Guard, _loans, _clock);

    private UpdateLoan Update => new(Guard, _loans);

    private DeleteLoan Delete => new(Guard, _loans);

    private ListLoans List => new(Guard, _loans);

    private void Retire() => _vehicle.ChangeStatus(VehicleStatus.Retired, _clock.Now);

    [Fact]
    public async Task Ajoute_un_emprunt_et_l_enregistre()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, Valid, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(_vehicle.Id, result.Value.VehicleId);
        Assert.Equal(48, result.Value.DurationMonths);
        Assert.Equal(_clock.Now, result.Value.RecordedAt);
        Assert.Single(_loans.Saved);
        Assert.Equal(1, _loans.SaveCount);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu()
    {
        var result = await Add.ExecuteAsync(Guid.NewGuid(), Valid, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
        Assert.Empty(_loans.Saved);
    }

    [Fact]
    public async Task Refuse_un_emprunt_invalide_sans_rien_enregistrer()
    {
        var result = await Add.ExecuteAsync(_vehicle.Id, Valid with { DurationMonths = 0 }, CancellationToken.None);

        Assert.Equal("loan.duration_invalid", result.Error!.Code);
        Assert.Equal(0, _loans.SaveCount);
    }

    [Fact]
    public async Task Hors_parc_refuse_la_creation_mais_autorise_lecture_modification_et_suppression()
    {
        var created = (await Add.ExecuteAsync(_vehicle.Id, Valid, CancellationToken.None)).Value;
        Retire();

        var add = await Add.ExecuteAsync(_vehicle.Id, Valid, CancellationToken.None);
        var list = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);
        var update = await Update.ExecuteAsync(_vehicle.Id, created.Id, Valid with { DurationMonths = 10 }, CancellationToken.None);
        var delete = await Delete.ExecuteAsync(_vehicle.Id, created.Id, CancellationToken.None);

        Assert.Equal(VehicleErrors.Retired, add.Error);
        Assert.Single(list.Value);
        Assert.Equal(10, update.Value.DurationMonths);
        Assert.True(delete.IsSuccess);
        Assert.Empty(_loans.Saved);
    }

    [Fact]
    public async Task Modifie_la_duree()
    {
        var created = (await Add.ExecuteAsync(_vehicle.Id, Valid, CancellationToken.None)).Value;

        var result = await Update.ExecuteAsync(_vehicle.Id, created.Id, Valid with { DurationMonths = 12 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Value.DurationMonths);
        Assert.Equal(12, _loans.Saved.Single().DurationMonths);
    }

    [Fact]
    public async Task Modification_ou_suppression_d_un_emprunt_inconnu()
    {
        var update = await Update.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), Valid, CancellationToken.None);
        var delete = await Delete.ExecuteAsync(_vehicle.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal("loan.not_found", update.Error!.Code);
        Assert.Equal("loan.not_found", delete.Error!.Code);
    }

    [Fact]
    public async Task Supprime_un_emprunt()
    {
        var created = (await Add.ExecuteAsync(_vehicle.Id, Valid, CancellationToken.None)).Value;

        var result = await Delete.ExecuteAsync(_vehicle.Id, created.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_loans.Saved);
    }

    [Fact]
    public async Task Ne_liste_que_les_emprunts_du_vehicule_du_plus_recent_au_plus_ancien()
    {
        var other = _vehicles.Seed("EF-456-GH");
        await Add.ExecuteAsync(_vehicle.Id, Valid with { StartDate = new DateOnly(2025, 1, 1) }, CancellationToken.None);
        await Add.ExecuteAsync(_vehicle.Id, Valid with { StartDate = new DateOnly(2026, 1, 1) }, CancellationToken.None);
        await Add.ExecuteAsync(other.Id, Valid, CancellationToken.None);

        var result = await List.ExecuteAsync(_vehicle.Id, CancellationToken.None);

        Assert.Equal([new DateOnly(2026, 1, 1), new DateOnly(2025, 1, 1)], result.Value.Select(l => l.StartDate));
    }

    [Fact]
    public async Task Liste_un_vehicule_inconnu_en_erreur()
    {
        var result = await List.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
    }
}
