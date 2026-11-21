using UsMecanic.Application.Documents;
using UsMecanic.Domain.Documents;

namespace UsMecanic.UnitTests.Application.Documents;

public class DocumentFileValidatorTests
{
    private static readonly byte[] Pdf = [.. "%PDF-1.7\n"u8, 0x01, 0x02];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    private static byte[] WithSize(byte[] signature, int size)
    {
        var bytes = new byte[size];
        signature.CopyTo(bytes, 0);
        return bytes;
    }

    [Fact]
    public void Reconnait_un_pdf()
    {
        var result = DocumentFileValidator.Validate(Pdf, "carte.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal(DocumentContentType.Pdf, result.Value.ContentType);
        Assert.Equal(Pdf.Length, result.Value.SizeBytes);
        Assert.Equal("carte.pdf", result.Value.FileName);
    }

    [Fact]
    public void Reconnait_un_jpeg()
    {
        Assert.Equal(DocumentContentType.Jpeg, DocumentFileValidator.Validate(Jpeg, "photo.jpg").Value.ContentType);
    }

    [Fact]
    public void Reconnait_un_png()
    {
        Assert.Equal(DocumentContentType.Png, DocumentFileValidator.Validate(Png, "scan.png").Value.ContentType);
    }

    [Fact]
    public void Le_type_vient_des_octets_et_pas_de_l_extension()
    {
        var result = DocumentFileValidator.Validate(Png, "photo.pdf");

        Assert.Equal(DocumentContentType.Png, result.Value.ContentType);
    }

    [Fact]
    public void Refuse_un_texte_renomme_en_pdf()
    {
        var result = DocumentFileValidator.Validate("Bonjour, ceci n'est pas un PDF"u8, "facture.pdf");

        Assert.Equal(DocumentErrors.FileTypeUnsupported, result.Error);
    }

    [Fact]
    public void Refuse_un_heic()
    {
        // En-tête ISO BMFF : boîte ftyp de marque heic.
        byte[] heic = [0x00, 0x00, 0x00, 0x18, .. "ftypheic"u8, 0x00, 0x00, 0x00, 0x00];

        Assert.Equal(DocumentErrors.FileTypeUnsupported, DocumentFileValidator.Validate(heic, "IMG_0001.jpg").Error);
    }

    [Fact]
    public void Refuse_un_zip()
    {
        byte[] zip = [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00];

        Assert.Equal(DocumentErrors.FileTypeUnsupported, DocumentFileValidator.Validate(zip, "archive.pdf").Error);
    }

    [Fact]
    public void Refuse_un_fichier_vide()
    {
        Assert.Equal(DocumentErrors.FileEmpty, DocumentFileValidator.Validate([], "vide.pdf").Error);
    }

    [Fact]
    public void Refuse_un_fichier_d_un_octet_sans_signature()
    {
        Assert.Equal(DocumentErrors.FileTypeUnsupported, DocumentFileValidator.Validate([0x41], "a.pdf").Error);
    }

    [Fact]
    public void Accepte_exactement_dix_mo()
    {
        var result = DocumentFileValidator.Validate(WithSize(Pdf, 10 * 1024 * 1024), "gros.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal(10 * 1024 * 1024, result.Value.SizeBytes);
    }

    [Fact]
    public void Refuse_dix_mo_et_un_octet()
    {
        var result = DocumentFileValidator.Validate(WithSize(Pdf, (10 * 1024 * 1024) + 1), "trop-gros.pdf");

        Assert.Equal(DocumentErrors.FileTooLarge, result.Error);
    }

    [Theory]
    [InlineData("C:\\Users\\jean\\Documents\\carte grise.pdf", "carte grise.pdf")]
    [InlineData("/home/jean/../secret/carte.pdf", "carte.pdf")]
    [InlineData("..\\..\\evil.pdf", "evil.pdf")]
    [InlineData("fa\u0000ct\nure\t.pdf", "facture.pdf")]
    [InlineData("  nom.pdf  ", "nom.pdf")]
    public void Nettoie_le_nom_d_origine(string original, string expected)
    {
        Assert.Equal(expected, DocumentFileValidator.Validate(Pdf, original).Value.FileName);
    }

    [Fact]
    public void Tronque_le_nom_a_255_caracteres()
    {
        var result = DocumentFileValidator.Validate(Pdf, new string('a', 300) + ".pdf");

        Assert.Equal(Document.MaxFileNameLength, result.Value.FileName.Length);
    }

    [Fact]
    public void La_troncature_ne_coupe_pas_une_paire_de_substitution()
    {
        var name = new string('a', 254) + "\U0001F698";

        var cleaned = DocumentFileValidator.SanitizeFileName(name);

        Assert.Equal(new string('a', 254), cleaned);
    }

    [Fact]
    public void La_troncature_garde_l_extension()
    {
        var result = DocumentFileValidator.Validate(Pdf, new string('a', 300) + ".pdf");

        Assert.Equal(new string('a', 251) + ".pdf", result.Value.FileName);
    }

    [Fact]
    public void Retire_les_caracteres_de_format_bidirectionnel()
    {
        var result = DocumentFileValidator.Validate(Pdf, "facture‮fdp.exe⁦.pdf");

        Assert.Equal("facturefdp.exe.pdf", result.Value.FileName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("dossier/")]
    [InlineData("..")]
    [InlineData("\u0001\u0002")]
    public void Remplace_un_nom_vide_apres_nettoyage_par_document_et_l_extension_du_type(string? original)
    {
        Assert.Equal("document.pdf", DocumentFileValidator.Validate(Pdf, original).Value.FileName);
        Assert.Equal("document.jpg", DocumentFileValidator.Validate(Jpeg, original).Value.FileName);
        Assert.Equal("document.png", DocumentFileValidator.Validate(Png, original).Value.FileName);
    }

    [Fact]
    public void Les_erreurs_ne_reprennent_pas_le_nom_du_fichier()
    {
        const string secret = "Jean-Dupont-contrat";
        var errors = new[]
        {
            DocumentFileValidator.Validate([], secret).Error!,
            DocumentFileValidator.Validate("texte"u8, secret).Error!,
            DocumentFileValidator.Validate(WithSize(Pdf, (10 * 1024 * 1024) + 1), secret).Error!,
        };

        Assert.All(errors, e => Assert.DoesNotContain(secret, e.Message + e.Code, StringComparison.Ordinal));
    }
}
