using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Companies;
using UsMecanic.Domain.Companies;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Companies;

internal sealed class CompanyRepository(UsMecanicDbContext db) : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Companies.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Company>> ListAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = db.Companies.AsNoTracking();
        if (!includeInactive)
        {
            query = query.Where(t => t.IsActive);
        }

        // Tri en mémoire : le référentiel compte quelques sociétés et le tri doit ignorer la casse.
        var companies = await query.ToListAsync(cancellationToken);
        return [.. companies.OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        // Comparaison en mémoire pour ignorer la casse sans dépendre de la traduction SQL.
        var companies = await db.Companies.AsNoTracking().ToListAsync(cancellationToken);
        return companies.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Company company) => db.Companies.Add(company);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
