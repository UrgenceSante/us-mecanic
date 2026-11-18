using UsMecanic.Domain.Companies;

namespace UsMecanic.Application.Companies;

public sealed record CompanyDto(Guid Id, string Name, bool IsActive)
{
    public static CompanyDto From(Company company)
    {
        ArgumentNullException.ThrowIfNull(company);

        return new(company.Id, company.Name, company.IsActive);
    }
}
