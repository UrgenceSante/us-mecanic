using System.Text;

namespace UsMecanic.Infrastructure.Vehicles;

/// <summary>
/// Découpage CSV (RFC 4180) : champs entre guillemets, guillemets doublés, retours à la ligne dans un champ.
/// Chaque enregistrement porte le numéro de la ligne où il commence.
/// </summary>
internal static class CsvRecordParser
{
    private static readonly char[] CandidateSeparators = [';', ',', '\t'];

    public sealed record CsvRecord(int Line, IReadOnlyList<string> Fields);

    public static IEnumerable<CsvRecord> Parse(string text, char separator)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var line = 1;
        var recordLine = 1;

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    line += c == '\n' ? 1 : 0;
                    field.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == separator)
            {
                fields.Add(field.ToString());
                field.Clear();
            }
            else if (c is '\n' or '\r')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                fields.Add(field.ToString());
                field.Clear();
                if (fields.Exists(f => f.Length > 0))
                {
                    yield return new CsvRecord(recordLine, [.. fields]);
                }

                fields.Clear();
                line++;
                recordLine = line;
            }
            else
            {
                field.Append(c);
            }
        }

        fields.Add(field.ToString());
        if (fields.Exists(f => f.Length > 0))
        {
            yield return new CsvRecord(recordLine, [.. fields]);
        }
    }

    /// <summary>Séparateur le plus fréquent dans la ligne d'en-tête (Excel FR exporte en « ; »).</summary>
    public static char DetectSeparator(string text)
    {
        var end = text.IndexOfAny(['\r', '\n']);
        var header = end < 0 ? text : text[..end];
        return CandidateSeparators.MaxBy(sep => header.Count(c => c == sep));
    }
}
