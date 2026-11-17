using UsMecanic.Domain.Companies;

namespace UsMecanic.Application.Companies;

/// <summary>Port de persistance des sociétés.</summary>
public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Sociétés triées par nom ; les sociétés désactivées ne sont incluses que sur demande.</summary>
    Task<IReadOnlyList<Company>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    /// <summary>Indique si une autre société (hors <paramref name="exceptId"/>) porte ce nom, sans tenir compte de la casse.</summary>
    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken);

    void Add(Company company);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
