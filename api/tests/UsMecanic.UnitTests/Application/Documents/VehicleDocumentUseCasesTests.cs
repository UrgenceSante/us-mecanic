using UsMecanic.Application.Documents;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Documents;
using UsMecanic.Domain.Vehicles;
using UsMecanic.UnitTests.TestDoubles;

namespace UsMecanic.UnitTests.Application.Documents;

public class VehicleDocumentUseCasesTests
{
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 0x00, 0xFF];

    private readonly InMemoryVehicleRepository _vehicles = new();
    private readonly InMemoryDocumentCategoryRepository _categories = new();
    private readonly InMemoryDocuments _documents = new();
    private readonly FixedClock _clock = new();

    private MileageVehicleGuard Guard => new(_vehicles);

    private AddVehicleDocument Add => new(Guard, _categories, _documents, _documents, _clock);

    [Fact]
    public async Task Ajoute_un_document_avec_son_contenu()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var category = _categories.Seed("Carte grise");

        var result = await Add.ExecuteAsync(vehicle.Id, category.Id, " Carte 2026 ", "C:\\scan\\carte.pdf", Pdf, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("carte.pdf", result.Value.FileName);
        Assert.Equal("Carte 2026", result.Value.Label);
        Assert.Equal("application/pdf", result.Value.ContentType);
        Assert.Equal(Pdf.Length, result.Value.SizeBytes);
        Assert.Equal(Pdf, await _documents.ReadAsync(result.Value.Id, CancellationToken.None));
        Assert.Single(_documents.Saved);
    }

    [Fact]
    public async Task Refuse_un_vehicule_inconnu()
    {
        var category = _categories.Seed("Carte grise");

        var result = await Add.ExecuteAsync(Guid.NewGuid(), category.Id, null, "a.pdf", Pdf, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
        Assert.Empty(_documents.Saved);
    }

    [Fact]
    public async Task Accepte_l_ajout_sur_un_vehicule_sorti_du_parc()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);
        var category = _categories.Seed("Autre");

        var result = await Add.ExecuteAsync(vehicle.Id, category.Id, null, "cession.pdf", Pdf, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refuse_une_categorie_absente_inconnue_ou_inactive()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var inactive = _categories.Seed("Ancienne", active: false);

        var missing = await Add.ExecuteAsync(vehicle.Id, null, null, "a.pdf", Pdf, CancellationToken.None);
        var unknown = await Add.ExecuteAsync(vehicle.Id, Guid.NewGuid(), null, "a.pdf", Pdf, CancellationToken.None);
        var disabled = await Add.ExecuteAsync(vehicle.Id, inactive.Id, null, "a.pdf", Pdf, CancellationToken.None);

        Assert.Equal(DocumentErrors.CategoryRequired, missing.Error);
        Assert.Equal(DocumentCategoryErrors.Unknown, unknown.Error);
        Assert.Equal(DocumentCategoryErrors.Inactive, disabled.Error);
        Assert.Empty(_documents.Saved);
    }

    [Fact]
    public async Task Refuse_un_fichier_vide_d_un_mauvais_type_ou_un_libelle_trop_long()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var category = _categories.Seed("Autre");

        var empty = await Add.ExecuteAsync(vehicle.Id, category.Id, null, "a.pdf", Array.Empty<byte>(), CancellationToken.None);
        var zip = await Add.ExecuteAsync(vehicle.Id, category.Id, null, "a.pdf", "PK\u0003\u0004"u8.ToArray(), CancellationToken.None);
        var label = await Add.ExecuteAsync(vehicle.Id, category.Id, new string('x', Document.MaxLabelLength + 1), "a.pdf", Pdf, CancellationToken.None);

        Assert.Equal(DocumentErrors.FileEmpty, empty.Error);
        Assert.Equal(DocumentErrors.FileTypeUnsupported, zip.Error);
        Assert.Equal(DocumentErrors.LabelTooLong, label.Error);
        Assert.Empty(_documents.Saved);
    }

    [Fact]
    public async Task Liste_du_plus_recent_au_plus_ancien_avec_filtre_et_sans_les_factures()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var other = _vehicles.Seed("EF-456-GH");
        var cat1 = _categories.Seed("Carte grise");
        var cat2 = _categories.Seed("Assurance");
        var old = _documents.Seed(vehicle.Id, cat1.Id, "old.pdf", FixedClock.Default.AddDays(-2));
        var recent = _documents.Seed(vehicle.Id, cat2.Id, "recent.pdf", FixedClock.Default);
        _documents.Seed(other.Id, cat1.Id, "autre.pdf", FixedClock.Default);
        _documents.SeedInvoice(vehicle.Id, Guid.NewGuid(), FixedClock.Default.AddDays(-1));
        var list = new ListVehicleDocuments(Guard, _documents);

        var all = await list.ExecuteAsync(vehicle.Id, null, CancellationToken.None);
        var filtered = await list.ExecuteAsync(vehicle.Id, cat1.Id, CancellationToken.None);

        Assert.Equal([recent.Id, old.Id], all.Value.Select(d => d.Id));
        Assert.Equal([old.Id], filtered.Value.Select(d => d.Id));
    }

    [Fact]
    public async Task La_liste_d_un_vehicule_inconnu_est_en_erreur()
    {
        var result = await new ListVehicleDocuments(Guard, _documents).ExecuteAsync(Guid.NewGuid(), null, CancellationToken.None);

        Assert.Equal(VehicleErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Lit_le_contenu_et_refuse_un_document_d_un_autre_vehicule()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var other = _vehicles.Seed("EF-456-GH");
        var document = _documents.Seed(vehicle.Id, Guid.NewGuid(), "a.pdf", FixedClock.Default, Pdf);
        var useCase = new GetVehicleDocumentContent(Guard, _documents, _documents);

        var ok = await useCase.ExecuteAsync(vehicle.Id, document.Id, CancellationToken.None);
        var wrongVehicle = await useCase.ExecuteAsync(other.Id, document.Id, CancellationToken.None);
        var unknown = await useCase.ExecuteAsync(vehicle.Id, Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(Pdf, ok.Value.Content);
        Assert.Equal("a.pdf", ok.Value.FileName);
        Assert.Equal("application/pdf", ok.Value.ContentType);
        Assert.Equal(DocumentErrors.NotFound, wrongVehicle.Error);
        Assert.Equal(DocumentErrors.NotFound, unknown.Error);
    }

    [Fact]
    public async Task Supprime_le_document_sans_appeler_la_suppression_du_contenu_et_meme_hors_parc()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        vehicle.ChangeStatus(VehicleStatus.Retired, FixedClock.Default);
        var document = _documents.Seed(vehicle.Id, Guid.NewGuid(), "a.pdf", FixedClock.Default);
        var useCase = new DeleteVehicleDocument(Guard, _documents);

        var result = await useCase.ExecuteAsync(vehicle.Id, document.Id, CancellationToken.None);
        var again = await useCase.ExecuteAsync(vehicle.Id, document.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_documents.Saved);
        Assert.False(_documents.HasContent(document.Id));
        Assert.Equal(0, _documents.DeleteContentCalls);
        Assert.Equal(DocumentErrors.NotFound, again.Error);
    }

    [Fact]
    public async Task Refuse_de_supprimer_un_document_d_un_autre_vehicule()
    {
        var vehicle = _vehicles.Seed("AB-123-CD");
        var other = _vehicles.Seed("EF-456-GH");
        var document = _documents.Seed(vehicle.Id, Guid.NewGuid(), "a.pdf", FixedClock.Default);

        var result = await new DeleteVehicleDocument(Guard, _documents).ExecuteAsync(other.Id, document.Id, CancellationToken.None);

        Assert.Equal(DocumentErrors.NotFound, result.Error);
        Assert.Single(_documents.Saved);
    }
}
