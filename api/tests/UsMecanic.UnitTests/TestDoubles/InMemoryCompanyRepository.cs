using UsMecanic.Application.Companies;
using UsMecanic.Domain.Companies;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des sociétés : les ajouts ne sont visibles qu'après SaveChangesAsync.</summary>
internal sealed class InMemoryCompanyRepository : ICompanyRepository
{
    private readonly List<Company> _pending = [];

    public List<Company> Saved { get; } = [];

    public int SaveCount { get; private set; }

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Find(t => t.Id == id));

    public Task<IReadOnlyList<Company>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Company>>([.. Saved.Where(t => includeInactive || t.IsActive).OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)]);

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Saved.Exists(t => t.Id != exceptId && string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase)));

    public void Add(Company company) => _pending.Add(company);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        SaveCount++;
        return Task.CompletedTask;
    }

    public Company Seed(string name, bool active = true)
    {
        var company = Company.Create(name).Value;
        if (!active)
        {
            company.Deactivate();
        }

        Saved.Add(company);
        return company;
    }
}
