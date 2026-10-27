using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Vehicles;

/// <summary>
/// Type d'utilisation d'un véhicule (ambulance, VSL, sang…). Référentiel administrable :
/// un type n'est jamais supprimé, seulement désactivé, pour rester affiché sur les véhicules qui l'ont.
/// </summary>
public sealed class VehicleType
{
    public const int MaxNameLength = 100;

    private VehicleType(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Un type inactif n'est plus proposé pour de nouvelles affectations.</summary>
    public bool IsActive { get; private set; }

    public static Result<VehicleType> Create(string? name)
    {
        var cleaned = NormalizeName(name);
        return cleaned.IsSuccess
            ? Result<VehicleType>.Success(new VehicleType(Guid.CreateVersion7(), cleaned.Value))
            : Result<VehicleType>.Failure(cleaned.Error!);
    }

    public Result<VehicleType> Rename(string? name)
    {
        var cleaned = NormalizeName(name);
        if (cleaned.IsFailure)
        {
            return Result<VehicleType>.Failure(cleaned.Error!);
        }

        Name = cleaned.Value;
        return Result<VehicleType>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Nettoie et valide un nom (obligatoire, longueur maximale) avant création ou renommage.</summary>
    public static Result<string> NormalizeName(string? name)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(VehicleTypeErrors.NameRequired);
        }

        return cleaned.Length > MaxNameLength
            ? Result<string>.Failure(VehicleTypeErrors.NameTooLong)
            : Result<string>.Success(cleaned);
    }
}
