using System.Text;
using UsMecanic.Infrastructure.Vehicles;

namespace UsMecanic.UnitTests.Infrastructure.Vehicles;

public class CsvVehicleImportReaderTests
{
    static CsvVehicleImportReaderTests() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    private static async Task<IReadOnlyList<UsMecanic.Application.Vehicles.Import.VehicleImportRow>> ReadAsync(byte[] bytes)
    {
        var result = await new CsvVehicleImportReader().ReadAsync(new MemoryStream(bytes), CancellationToken.None);
        Assert.True(result.IsSuccess, result.Error?.Message);
        return result.Value;
    }

    private static Task<IReadOnlyList<UsMecanic.Application.Vehicles.Import.VehicleImportRow>> ReadAsync(string csv) =>
        ReadAsync(Encoding.UTF8.GetBytes(csv));

    [Fact]
    public async Task Lit_un_export_Excel_francais()
    {
        var rows = await ReadAsync("Immatriculation;Marque;Modèle;Mise en circulation;N° de série;Indicatif\r\n"
            + "AB-123-CD;Renault;Master;15/03/2021;VF1MA000012345678;AMB-1\r\n"
            + "\r\n"
            + "EF-456-GH;Peugeot;;;;\r\n");

        Assert.Equal(2, rows.Count);
        var first = rows[0];
        Assert.Equal((2, "AB-123-CD", "Renault", "Master", "15/03/2021", "VF1MA000012345678", "AMB-1"),
            (first.Line, first.LicensePlate, first.Brand, first.Model, first.FirstRegistrationDate, first.Vin, first.InternalNumber));
        Assert.Equal(4, rows[1].Line);
        Assert.Null(rows[1].Model);
    }

    [Fact]
    public async Task Gere_la_virgule_les_guillemets_et_les_colonnes_inconnues()
    {
        var rows = await ReadAsync("commentaire,immat,marque\n\"ligne 1\nsuite, avec \"\"guillemets\"\"\",AB-123-CD,\"Renault, Trucks\"\nx,EF-456-GH");

        Assert.Equal(2, rows.Count);
        Assert.Equal(("AB-123-CD", "Renault, Trucks", 2), (rows[0].LicensePlate, rows[0].Brand, rows[0].Line));
        Assert.Equal(4, rows[1].Line);
    }

    [Fact]
    public async Task Lit_l_UTF8_avec_BOM_et_le_Windows_1252()
    {
        var withBom = await ReadAsync([.. Encoding.UTF8.Preamble, .. Encoding.UTF8.GetBytes("immatriculation;modèle\nAB-123-CD;Clio Société")]);
        var ansi = await ReadAsync(Encoding.GetEncoding(1252).GetBytes("immatriculation;modèle\nAB-123-CD;Clio Société"));

        Assert.Equal("Clio Société", withBom[0].Model);
        Assert.Equal("Clio Société", ansi[0].Model);
    }

    [Fact]
    public async Task Refuse_un_fichier_sans_colonne_immatriculation()
    {
        var result = await new CsvVehicleImportReader().ReadAsync(new MemoryStream("marque;modele\nRenault;Master"u8.ToArray()), CancellationToken.None);

        Assert.Equal(CsvVehicleImportReader.MissingPlateColumn, result.Error);
    }
}
