using System.Reflection;
using UsMecanic.Application.Documents;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Documents;
using UsMecanic.Domain.Maintenance;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Documents;

public class InvoiceUseCasesTests
{
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 0x00, 0xFF];
    private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04];
    private static readonly DateOnly Day = new(2026, 3, 5);

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryInterventionRepository _interventions = new();
    private readonly InMemoryExpenseRepository _expenses = new();
    private readonly InMemoryDocuments _documents = new();
    private readonly FixedClock _clock = new();
    private readonly Vehicle _vehicle;

    public InvoiceUseCasesTests()
    {
        _vehicle = _vehicles.Seed("AB-123-CD");
    }

    private MileageVehicleGuard Guard => new(_vehicles);

    private AddInvoice Add => new(Guard, _interventions, _expenses, _documents, _documents, _clock);

    private ListInvoices List => new(Guard, _interventions, _expenses, _documents);

    [Fact]
    public async Task Ajoute_plusieurs_factures_a_une_intervention_sans_categorie()
    {
        var intervention = _interventions.SeedFor(_vehicle.Id, Day);

        var first = await Add.ForInterventionAsync(_vehicle.Id, intervention.Id, " Facture ", "C:\\scan\\f.pdf", Pdf, CancellationToken.None);
        var second = await Add.ForInterventionAsync(_vehicle.Id, intervention.Id, null, "devis.pdf", Pdf, CancellationToken.None);
        var list = await List.ForInterventionAsync(_vehicle.Id, intervention.Id, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal("Facture", first.Value.Label);
        Assert.Equal("f.pdf", first.Value.FileName);
        Assert.Equal(intervention.Id, first.Value.InterventionId);
        Assert.Null(first.Value.CategoryId);
        Assert.Null(first.Value.ExpenseId);
        Assert.Equal(Pdf, await _documents.ReadAsync(first.Value.Id, CancellationToken.None));
        Assert.Equal(2, list.Value.Count);
        Assert.Contains(second.Value.Id, list.Value.Select(d => d.Id));
    }

    [Fact]
    public async Task Ajoute_une_facture_a_une_depense_ponctuelle()
    {
        var expense = _expenses.SeedFor(_vehicle.Id, Day);

        var result = await Add.ForExpenseAsync(_vehicle.Id, expense.Id, null, "facture.pdf", Pdf, CancellationToken.None);
        var list = await List.ForExpenseAsync(_vehicle.Id, expense.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expense.Id, result.Value.ExpenseId);
        Assert.Null(result.Value.InterventionId);
        Assert.Single(list.Value);
    }

    [Fact]
    public async Task Refuse_une_intervention_ou_une_depense_inconnue_ou_d_un_autre_vehicule()
    {
        var other = _vehicles.Seed("EF-456-GH");
        var foreignIntervention = _interventions.SeedFor(other.Id, Day);
        var foreignExpense = _expenses.SeedFor(other.Id, Day);

        var unknownIntervention = await Add.ForInterventionAsync(_vehicle.Id, Guid.NewGuid(), null, "a.pdf", Pdf, CancellationToken.None);
        var otherIntervention = await Add.ForInterventionAsync(_vehicle.Id, foreignIntervention.Id, null, "a.pdf", Pdf, CancellationToken.None);
        var unknownExpense = await Add.ForExpenseAsync(_vehicle.Id, Guid.NewGuid(), null, "a.pdf", Pdf, CancellationToken.None);
        var otherExpense = await Add.ForExpenseAsync(_vehicle.Id, foreignExpense.Id, null, "a.pdf", Pdf, CancellationToken.None);
        var listOther = await List.ForInterventionAsync(_vehicle.Id, foreignIntervention.Id, CancellationToken.None);

        Assert.Equal(InterventionErrors.NotFound, unknownIntervention.Error);
        Assert.Equal(InterventionErrors.NotFound, otherIntervention.Error);
        Assert.Equal(ExpenseErrors.NotFound, unknownExpense.Error);
        Assert.Equal(ExpenseErrors.NotFound, otherExpense.Error);
        Assert.Equal(InterventionErrors.NotFound, listOther.Error);
        Assert.Empty(_documents.Saved);
    }

    [Fact]
    public async Task Refuse_une_depense_importee_par_un_connecteur()
    {
        var expense = _expenses.SeedFor(_vehicle.Id, Day);
        typeof(Expense).GetField("<Source>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(expense, ExpenseSource.Total);

        var result = await Add.ForExpenseAsync(_vehicle.Id, expense.Id, null, "a.pdf", Pdf, CancellationToken.None);

        Assert.Equal(DocumentErrors.ExpenseNotAttachable, result.Error);
        Assert.Empty(_documents.Saved);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu_et_accepte_un_vehicule_sorti_du_parc()
    {
        var intervention = _interventions.SeedFor(_vehicle.Id, Day);
        var unknown = await Add.ForInterventionAsync(Guid.NewGuid(), intervention.Id, null, "a.pdf", Pdf, CancellationToken.None);
        _vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);

        var late = await Add.ForInterventionAsync(_vehicle.Id, intervention.Id, null, "a.pdf", Pdf, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, unknown.Error);
        Assert.True(late.IsSuccess);
    }

    [Fact]
    public async Task Refuse_un_fichier_vide_d_un_mauvais_type_ou_un_libelle_trop_long()
    {
        var intervention = _interventions.SeedFor(_vehicle.Id, Day);

        var empty = await Add.ForInterventionAsync(_vehicle.Id, intervention.Id, null, "a.pdf", ReadOnlyMemory<byte>.Empty, CancellationToken.None);
        var zip = await Add.ForInterventionAsync(_vehicle.Id, intervention.Id, null, "a.pdf", Zip, CancellationToken.None);
        var label = await Add.ForInterventionAsync(_vehicle.Id, intervention.Id, new string('x', 201), "a.pdf", Pdf, CancellationToken.None);

        Assert.Equal(DocumentErrors.FileEmpty, empty.Error);
        Assert.Equal(DocumentErrors.FileTypeUnsupported, zip.Error);
        Assert.Equal(DocumentErrors.LabelTooLong, label.Error);
        Assert.Empty(_documents.Saved);
    }

    [Fact]
    public async Task Le_comptage_des_documents_se_fait_en_une_requete_par_liste()
    {
        var first = _interventions.SeedFor(_vehicle.Id, Day);
        var second = _interventions.SeedFor(_vehicle.Id, Day.AddDays(1));
        _interventions.SeedFor(_vehicle.Id, Day.AddDays(2));
        _documents.SeedInvoice(_vehicle.Id, first.Id, FixedClock.Default);
        _documents.SeedInvoice(_vehicle.Id, first.Id, FixedClock.Default);
        _documents.SeedInvoice(_vehicle.Id, second.Id, FixedClock.Default);
        var factory = new InterventionDtoFactory(
            new InMemoryGarageRepository(), new InMemoryVehicleDeadlineRepository(), new InMemoryDeadlineTypeRepository(), _documents);

        var dtos = await factory.BuildAsync(_vehicle.Id, await _interventions.ListByVehicleAsync(_vehicle.Id, CancellationToken.None), CancellationToken.None);

        Assert.Equal([0, 1, 2], dtos.Select(d => d.DocumentCount).Order());
        Assert.Equal(1, _documents.CountCalls);
    }
}
