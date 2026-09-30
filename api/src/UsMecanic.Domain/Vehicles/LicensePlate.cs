using System.Text.RegularExpressions;
using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Vehicles;

/// <summary>
/// Immatriculation d'un véhicule au format SIV (AA-123-AA).
/// La saisie est normalisée : majuscules, séparateurs (espaces, tirets) facultatifs.
/// </summary>
public sealed partial record LicensePlate
{
    public static readonly DomainError InvalidFormat =
        new("license_plate.invalid_format", "L'immatriculation doit respecter le format SIV AA-123-AA.");

    private LicensePlate(string value) => Value = value;

    /// <summary>Forme canonique : AA-123-AA.</summary>
    public string Value { get; }

    public static Result<LicensePlate> Create(string? input)
    {
        var compact = SeparatorsRegex().Replace(input ?? string.Empty, string.Empty).ToUpperInvariant();

        var match = SivRegex().Match(compact);
        if (!match.Success)
        {
            return Result<LicensePlate>.Failure(InvalidFormat);
        }

        var canonical = $"{match.Groups[1].Value}-{match.Groups[2].Value}-{match.Groups[3].Value}";
        return Result<LicensePlate>.Success(new LicensePlate(canonical));
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[\s-]+")]
    private static partial Regex SeparatorsRegex();

    [GeneratedRegex("^([A-Z]{2})([0-9]{3})([A-Z]{2})$")]
    private static partial Regex SivRegex();
}
