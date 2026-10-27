using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Vehicles;

public class ImportVehiclesTests
{
    private readonly InMemoryVehicleRepository _repository = new();
    private readonly FixedClock _clock = new();

    private sealed class StubReader(params VehicleImportRow[] rows) : IVehicleImportReader
    {
        public Task<Result<IReadOnlyList<VehicleImportRow>>> ReadAsync(Stream content, CancellationToken cancellationToken) =>
            Task.FromResult(Result<IReadOnlyList<VehicleImportRow>>.Success(rows));
    }

    private async Task<VehicleImportReport> ImportAsync(bool dryRun, params VehicleImportRow[] rows)
    {
        var useCase = new ImportVehicles(new StubReader(rows), _repository, _clock, _clock.Calendar);
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
        var useCase = new ImportVehicles(new StubReader(), _repository, _clock, _clock.Calendar);

        var result = await useCase.ExecuteAsync(Stream.Null, false, CancellationToken.None);

        Assert.Equal(ImportErrors.Empty, result.Error);
    }
}
