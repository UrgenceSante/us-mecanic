using System.Text.RegularExpressions;
using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Vehicles;

/// <summary>
/// Numéro d'identification du véhicule (VIN, champ E de la carte grise) :
/// 17 caractères alphanumériques, sans I, O ni Q.
/// </summary>
public sealed partial record Vin
{
    public static readonly DomainError InvalidFormat =
        new("vin.invalid_format", "Le VIN doit comporter 17 caractères alphanumériques (sans I, O ni Q).");

    private Vin(string value) => Value = value;

    public string Value { get; }

    public static Result<Vin> Create(string? input)
    {
        var normalized = (input ?? string.Empty).Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

        return VinRegex().IsMatch(normalized)
            ? Result<Vin>.Success(new Vin(normalized))
            : Result<Vin>.Failure(InvalidFormat);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-HJ-NPR-Z0-9]{17}$")]
    private static partial Regex VinRegex();
}
