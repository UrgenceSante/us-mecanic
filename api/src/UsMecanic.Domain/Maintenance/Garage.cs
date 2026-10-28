using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Maintenance;

/// <summary>
/// Garage où sont réalisées les interventions d'entretien. Référentiel administrable :
/// un garage n'est jamais supprimé, seulement désactivé, pour rester affiché sur les interventions qui l'ont.
/// </summary>
public sealed class Garage
{
    public const int MaxNameLength = 100;

    private Garage(Guid id, string name)
    {
        Id = id;
        Name = name;
        IsActive = true;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    /// <summary>Un garage inactif n'est plus proposé pour de nouvelles affectations.</summary>
    public bool IsActive { get; private set; }

    public static Result<Garage> Create(string? name)
    {
        var cleaned = NormalizeName(name);
        return cleaned.IsSuccess
            ? Result<Garage>.Success(new Garage(Guid.CreateVersion7(), cleaned.Value))
            : Result<Garage>.Failure(cleaned.Error!);
    }

    public Result<Garage> Rename(string? name)
    {
        var cleaned = NormalizeName(name);
        if (cleaned.IsFailure)
        {
            return Result<Garage>.Failure(cleaned.Error!);
        }

        Name = cleaned.Value;
        return Result<Garage>.Success(this);
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Nettoie et valide un nom (obligatoire, longueur maximale) avant création ou renommage.</summary>
    public static Result<string> NormalizeName(string? name)
    {
        var cleaned = name?.Trim();
        if (string.IsNullOrEmpty(cleaned))
        {
            return Result<string>.Failure(GarageErrors.NameRequired);
        }

        return cleaned.Length > MaxNameLength
            ? Result<string>.Failure(GarageErrors.NameTooLong)
            : Result<string>.Success(cleaned);
    }
}
