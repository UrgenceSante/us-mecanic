using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Rentals;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class ImportVehiclesTests
{
    private readonly InMemoryVehicleRepository _repository = new();
    private readonly InMemoryCompanyRepository _companies = new();
    private readonly InMemoryRentalRepository _rentals = new();
    private readonly FixedClock _clock = new();

    private sealed class StubReader(params VehicleImportRow[] rows) : IVehicleImportReader
    {
        public Task<Result<IReadOnlyList<VehicleImportRow>>> ReadAsync(Stream content, CancellationToken cancellationToken) =>
            Task.FromResult(Result<IReadOnlyList<VehicleImportRow>>.Success(rows));
    }

    private async Task<VehicleImportReport> ImportAsync(bool dryRun, params VehicleImportRow[] rows)
    {
        var useCase = new ImportVehicles(new StubReader(rows), _repository, _companies, _rentals, _clock, _clock.Calendar);
        var result = await useCase.ExecuteAsync(Stream.Null, dryRun, CancellationToken.None);
        Assert.True(result.IsSuccess, result.Error?.Message);
        return result.Value;
    }

    [Fact]
    public async Task Cree_les_vehicules_inconnus()
    {
        var report = await ImportAsync(false,
            new VehicleImportRow(2, "AB-123-CD", "Renault", "Master", "15/03/2021", null, "AMB-1"),
            new VehicleImportRow(3, "EF-456-GH", FirstRegistrationDate: "2020-01-31"));

        Assert.Equal(2, report.Created);
        Assert.Equal(2, _repository.Saved.Count);
        var first = _repository.Saved.Single(v => v.LicensePlate.Value == "AB-123-CD");
        Assert.Equal(new DateOnly(2021, 3, 15), first.FirstRegistrationDate);
        Assert.Equal("AMB-1", first.InternalNumber);
    }

    [Fact]
    public async Task Met_a_jour_sans_effacer_les_valeurs_absentes_du_fichier()
    {
        var existing = _repository.Seed("AB-123-CD", new VehicleDetails("Renault", "Master", null, null, "AMB-1"));

        var report = await ImportAsync(false, new VehicleImportRow(2, "ab123cd", Model: "Master L3H2"));

        Assert.Equal(ImportOutcome.Updated, Assert.Single(report.Lines).Outcome);
        Assert.Equal("Renault", existing.Brand);
        Assert.Equal("Master L3H2", existing.Model);
        Assert.Equal("AMB-1", existing.InternalNumber);
    }

    [Fact]
    public async Task Conserve_le_type_d_un_vehicule_existant()
    {
        var typeId = Guid.NewGuid();
        var existing = _repository.Seed("AB-123-CD", new VehicleDetails("Renault", null, null, null, null, typeId));

        var report = await ImportAsync(false, new VehicleImportRow(2, "AB-123-CD", Model: "Master"));

        Assert.Equal(ImportOutcome.Updated, Assert.Single(report.Lines).Outcome);
        Assert.Equal(typeId, existing.TypeId);
    }

    [Fact]
    public async Task Signale_les_lignes_identiques_a_l_existant()
    {
        _repository.Seed("AB-123-CD", new VehicleDetails("Renault", null, null, null, null));

        var report = await ImportAsync(false, new VehicleImportRow(2, "AB-123-CD", "Renault"));

        Assert.Equal(1, report.Unchanged);
        Assert.Equal(0, _repository.SaveCount);
    }

    [Fact]
    public async Task Rapporte_les_lignes_en_erreur_et_importe_les_autres()
    {
        _repository.Seed("ZZ-999-ZZ").ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var report = await ImportAsync(false,
            new VehicleImportRow(2, "AB-123-CD"),
            new VehicleImportRow(3, "n'importe quoi"),
            new VehicleImportRow(4, "AB123CD"),
            new VehicleImportRow(5, "EF-456-GH", FirstRegistrationDate: "31/02/2021"),
            new VehicleImportRow(6, "IJ-789-KL", Vin: "COURT"),
            new VehicleImportRow(7, "ZZ-999-ZZ", "Peugeot"));

        Assert.Equal(1, report.Created);
        Assert.Equal(5, report.Errors);
        Assert.Equal([2, 3, 4, 5, 6, 7], report.Lines.Select(l => l.Line));
        Assert.Equal(
            [LicensePlate.InvalidFormat.Code, ImportErrors.DuplicatePlate.Code, ImportErrors.InvalidDate.Code, Vin.InvalidFormat.Code, VehicleErrors.Retired.Code],
            report.Lines.Where(l => l.Outcome == ImportOutcome.Error).Select(l => l.ErrorCode));
        Assert.Equal(2, _repository.Saved.Count);
    }

    [Fact]
    public async Task La_simulation_n_enregistre_rien()
    {
        var report = await ImportAsync(true, new VehicleImportRow(2, "AB-123-CD"));

        Assert.True(report.DryRun);
        Assert.Equal(1, report.Created);
        Assert.Equal(0, _repository.SaveCount);
        Assert.Empty(_repository.Saved);
    }

    [Fact]
    public async Task Refuse_un_fichier_vide()
    {
        var useCase = new ImportVehicles(new StubReader(), _repository, _companies, _rentals, _clock, _clock.Calendar);

        var result = await useCase.ExecuteAsync(Stream.Null, false, CancellationToken.None);

        Assert.Equal(ImportErrors.Empty, result.Error);
    }

    [Fact]
    public async Task Rapproche_le_proprietaire_par_nom_sans_tenir_compte_de_la_casse()
    {
        var company = _companies.Seed("Ambulances Nord");
        var existing = _repository.Seed("EF-456-GH");

        var report = await ImportAsync(false,
            new VehicleImportRow(2, "AB-123-CD", Owner: "  ambulances NORD "),
            new VehicleImportRow(3, "EF-456-GH", Owner: "Ambulances Nord"));

        Assert.Equal(1, report.Created);
        Assert.Equal(1, report.Updated);
        Assert.Equal(company.Id, _repository.Saved.Single(v => v.LicensePlate.Value == "AB-123-CD").OwnerCompanyId);
        Assert.Equal(company.Id, existing.OwnerCompanyId);
    }

    [Fact]
    public async Task Met_en_erreur_une_societe_inconnue_ou_inactive_sans_la_creer()
    {
        _companies.Seed("Ancienne société", active: false);

        var report = await ImportAsync(false,
            new VehicleImportRow(2, "AB-123-CD", Owner: "Inconnue"),
            new VehicleImportRow(3, "EF-456-GH", Owner: "Ancienne société"));

        Assert.Equal(2, report.Errors);
        Assert.All(report.Lines, l => Assert.Equal(ImportErrors.UnknownCompany("x").Code, l.ErrorCode));
        Assert.Equal("Société inconnue ou inactive : Inconnue", report.Lines[0].Message);
        Assert.Empty(_repository.Saved);
        Assert.Single(_companies.Saved);
    }

    [Fact]
    public async Task Une_cellule_proprietaire_vide_conserve_le_proprietaire_existant()
    {
        var company = _companies.Seed("Ambulances Nord");
        var existing = _repository.Seed("AB-123-CD", new VehicleDetails("Renault", null, null, null, null, null, company.Id));

        var report = await ImportAsync(false, new VehicleImportRow(2, "AB-123-CD", Model: "Master"));

        Assert.Equal(ImportOutcome.Updated, Assert.Single(report.Lines).Outcome);
        Assert.Equal(company.Id, existing.OwnerCompanyId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Refuse_un_proprietaire_locataire_en_cours_ou_a_venir(bool dryRun)
    {
        var tenant = _companies.Seed("Locataire");
        var vehicle = _repository.Seed("AB-123-CD");
        _rentals.Seed(vehicle.Id, tenant.Id, FixedClock.DefaultToday.AddDays(10), null);

        var report = await ImportAsync(dryRun, new VehicleImportRow(2, "AB-123-CD", Owner: "Locataire"));

        var line = Assert.Single(report.Lines);
        Assert.Equal(ImportOutcome.Error, line.Outcome);
        Assert.Equal(RentalErrors.OwnerIsTenant("Locataire").Code, line.ErrorCode);
        Assert.Null(vehicle.OwnerCompanyId);
    }

    [Fact]
    public async Task Accepte_un_proprietaire_dont_la_location_est_terminee()
    {
        var company = _companies.Seed("Ancien locataire");
        var vehicle = _repository.Seed("AB-123-CD");
        _rentals.Seed(vehicle.Id, company.Id, FixedClock.DefaultToday.AddDays(-30), FixedClock.DefaultToday.AddDays(-1));

        var report = await ImportAsync(false, new VehicleImportRow(2, "AB-123-CD", Owner: "Ancien locataire"));

        Assert.Equal(1, report.Updated);
        Assert.Equal(company.Id, vehicle.OwnerCompanyId);
    }

    [Fact]
    public async Task Accepte_le_proprietaire_actuel_meme_desactive()
    {
        var company = _companies.Seed("Société dissoute");
        _repository.Seed("AB-123-CD", new VehicleDetails("Renault", null, null, null, null, null, company.Id));
        company.Deactivate();

        var report = await ImportAsync(false, new VehicleImportRow(2, "AB-123-CD", Model: "Master", Owner: "Société dissoute"));

        var line = Assert.Single(report.Lines);
        Assert.Equal(ImportOutcome.Updated, line.Outcome);
        Assert.Equal("Société dissoute", line.Owner);
    }

    [Fact]
    public async Task Le_bilan_indique_le_proprietaire_applique()
    {
        var company = _companies.Seed("Ambulances Nord");
        _repository.Seed("EF-456-GH", new VehicleDetails("Renault", null, null, null, null, null, company.Id));

        var report = await ImportAsync(true,
            new VehicleImportRow(2, "AB-123-CD", Owner: "ambulances nord"),
            new VehicleImportRow(3, "EF-456-GH", Model: "Master"),
            new VehicleImportRow(4, "IJ-789-KL"));

        Assert.Equal(["Ambulances Nord", "Ambulances Nord", null], report.Lines.Select(l => l.Owner));
    }
}
