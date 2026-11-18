namespace UsMecanic.Application.Companies;

/// <summary>Liste les sociétés. Les sociétés désactivées ne sont plus proposées, sauf demande explicite.</summary>
public sealed class ListCompanies(ICompanyRepository repository)
{
    public async Task<IReadOnlyList<CompanyDto>> ExecuteAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var companies = await repository.ListAsync(includeInactive, cancellationToken);
        return [.. companies.Select(CompanyDto.From)];
    }
}
