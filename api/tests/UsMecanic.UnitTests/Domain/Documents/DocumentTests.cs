using UsMecanic.Domain.Documents;

namespace UsMecanic.UnitTests.Domain.Documents;

public class DocumentTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 10, 0, 0, TimeSpan.Zero);
    private static readonly DocumentFile File = new("carte.pdf", DocumentContentType.Pdf, 1234);

    [Fact]
    public void Un_document_du_vehicule_garde_sa_categorie_et_n_a_aucun_rattachement()
    {
        var vehicleId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var result = Document.CreateForVehicle(vehicleId, categoryId, "  Carte grise 2026 ", File, Now);

        Assert.True(result.IsSuccess);
        var document = result.Value;
        Assert.Equal(vehicleId, document.VehicleId);
        Assert.Equal(categoryId, document.CategoryId);
        Assert.Null(document.InterventionId);
        Assert.Null(document.ExpenseId);
        Assert.Equal("Carte grise 2026", document.Label);
        Assert.Equal("carte.pdf", document.FileName);
        Assert.Equal(DocumentContentType.Pdf, document.ContentType);
        Assert.Equal(1234, document.SizeBytes);
        Assert.Equal(Now, document.AddedAt);
    }

    [Fact]
    public void Un_document_du_vehicule_exige_une_categorie()
    {
        Assert.Equal(DocumentErrors.CategoryRequired, Document.CreateForVehicle(Guid.NewGuid(), null, null, File, Now).Error);
    }

    [Fact]
    public void Une_facture_d_intervention_n_a_pas_de_categorie()
    {
        var interventionId = Guid.NewGuid();

        var document = Document.CreateInvoice(Guid.NewGuid(), interventionId, null, null, File, Now).Value;

        Assert.Equal(interventionId, document.InterventionId);
        Assert.Null(document.ExpenseId);
        Assert.Null(document.CategoryId);
    }

    [Fact]
    public void Une_facture_de_depense_n_a_pas_de_categorie()
    {
        var expenseId = Guid.NewGuid();

        var document = Document.CreateInvoice(Guid.NewGuid(), null, expenseId, null, File, Now).Value;

        Assert.Equal(expenseId, document.ExpenseId);
        Assert.Null(document.InterventionId);
        Assert.Null(document.CategoryId);
    }

    [Fact]
    public void Une_facture_exige_un_rattachement()
    {
        Assert.Equal(DocumentErrors.AttachmentRequired, Document.CreateInvoice(Guid.NewGuid(), null, null, null, File, Now).Error);
    }

    [Fact]
    public void Une_facture_ne_se_rattache_pas_aux_deux()
    {
        var result = Document.CreateInvoice(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, File, Now);

        Assert.Equal(DocumentErrors.AttachmentAmbiguous, result.Error);
    }

    [Fact]
    public void Un_libelle_vide_devient_absent_et_un_libelle_trop_long_est_refuse()
    {
        Assert.Null(Document.CreateForVehicle(Guid.NewGuid(), Guid.NewGuid(), "   ", File, Now).Value.Label);

        var tooLong = new string('x', Document.MaxLabelLength + 1);
        Assert.Equal(DocumentErrors.LabelTooLong, Document.CreateForVehicle(Guid.NewGuid(), Guid.NewGuid(), tooLong, File, Now).Error);
    }
}
