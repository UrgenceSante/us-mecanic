using System.Text;
using UsMecanic.Application.Vehicles.Import;
using UsMecanic.Domain.Common;

namespace UsMecanic.Infrastructure.Vehicles;

/// <summary>
/// Lit un fichier CSV de véhicules. La première ligne est l'en-tête ; seule la colonne immatriculation
/// est obligatoire, les colonnes inconnues sont ignorées. Les intitulés sont comparés sans casse,
/// accents, espaces ni ponctuation (« Mise en circulation » = « mise_en_circulation »).
/// Encodage : UTF-8, ou Windows-1252 à défaut (exports Excel).
/// </summary>
internal sealed class CsvVehicleImportReader : IVehicleImportReader
{
    public static readonly DomainError MissingPlateColumn = new(
        "import.missing_plate_column",
        "Colonne « immatriculation » introuvable dans l'en-tête du fichier.");

    private enum Column { Plate, Brand, Model, FirstRegistration, Vin, InternalNumber }

    /// <summary>Intitulés reconnus, sous forme normalisée (cf. <see cref="NormalizeHeader"/>).</summary>
    private static readonly Dictionary<string, Column> HeaderAliases = new (Column Column, string[] Names)[]
    {
        (Column.Plate, ["immatriculation", "immat", "plaque", "licenseplate"]),
        (Column.Brand, ["marque", "brand"]),
        (Column.Model, ["modele", "model"]),
        (Column.FirstRegistration, ["miseencirculation", "datemiseencirculation", "datedemiseencirculation", "mec", "firstregistrationdate"]),
        (Column.Vin, ["vin", "numeroserie", "numerodeserie", "nserie", "ndeserie"]),
        (Column.InternalNumber, ["numerointerne", "ninterne", "indicatif", "numeroparc", "internalnumber"]),
    }.SelectMany(alias => alias.Names.Select(name => (name, alias.Column))).ToDictionary(alias => alias.name, alias => alias.Column);

    private static readonly Dictionary<char, char> AccentFolding = "àâäáãéèêëíîïìóôöòõúûüùçñÿ"
        .Zip("aaaaaeeeeiiiiooooouuuucny")
        .ToDictionary(pair => pair.First, pair => pair.Second);

    private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, throwOnInvalidBytes: true);

    static CsvVehicleImportReader() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    public async Task<Result<IReadOnlyList<VehicleImportRow>>> ReadAsync(Stream content, CancellationToken cancellationToken)
    {
        var text = await ReadTextAsync(content, cancellationToken);
        var records = CsvRecordParser.Parse(text, CsvRecordParser.DetectSeparator(text)).ToList();
        if (records.Count == 0)
        {
            return Result<IReadOnlyList<VehicleImportRow>>.Success([]);
        }

        var columns = MapHeader(records[0].Fields);
        if (!columns.ContainsKey(Column.Plate))
        {
            return Result<IReadOnlyList<VehicleImportRow>>.Failure(MissingPlateColumn);
        }

        var rows = records.Skip(1).Select(record =>
        {
            string? Get(Column column) =>
                columns.TryGetValue(column, out var index) && index < record.Fields.Count && !string.IsNullOrWhiteSpace(record.Fields[index])
                    ? record.Fields[index].Trim()
                    : null;

            return new VehicleImportRow(record.Line, Get(Column.Plate), Get(Column.Brand), Get(Column.Model),
                Get(Column.FirstRegistration), Get(Column.Vin), Get(Column.InternalNumber));
        });

        return Result<IReadOnlyList<VehicleImportRow>>.Success([.. rows]);
    }

    private static Dictionary<Column, int> MapHeader(IReadOnlyList<string> header)
    {
        var columns = new Dictionary<Column, int>();
        for (var i = 0; i < header.Count; i++)
        {
            if (HeaderAliases.TryGetValue(NormalizeHeader(header[i]), out var column))
            {
                columns.TryAdd(column, i);
            }
        }

        return columns;
    }

    /// <summary>
    /// Minuscules, sans accents ni caractères non alphanumériques. Table explicite plutôt que
    /// <see cref="string.Normalize()"/> : l'image Alpine tourne en globalisation invariante.
    /// </summary>
    private static string NormalizeHeader(string header) => string.Concat(header
        .ToLowerInvariant()
        .Select(c => AccentFolding.GetValueOrDefault(c, c))
        .Where(char.IsAsciiLetterOrDigit));

    private static async Task<string> ReadTextAsync(Stream content, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        var bytes = buffer.ToArray();
        var offset = bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble) ? Encoding.UTF8.Preamble.Length : 0;

        try
        {
            return StrictUtf8.GetString(bytes, offset, bytes.Length - offset);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.GetEncoding(1252).GetString(bytes);
        }
    }
}
