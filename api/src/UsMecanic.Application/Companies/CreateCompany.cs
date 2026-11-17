using UsMecanic.Domain.Common;
using UsMecanic.Domain.Companies;

namespace UsMecanic.Application.Companies;

/// <summary>Ajoute une société. Le nom est obligatoire et unique (sans tenir compte de la casse).</summary>
public sealed class CreateCompany(ICompanyRepository repository)
{
    public async Task<Result<CompanyDto>> ExecuteAsync(string? name, CancellationToken cancellationToken)
    {
        var company = Company.Create(name);
        if (company.IsFailure)
        {
            return Result<CompanyDto>.Failure(company.Error!);
        }

        if (await repository.NameExistsAsync(company.Value.Name, null, cancellationToken))
        {
            return Result<CompanyDto>.Failure(CompanyErrors.NameAlreadyExists);
        }

        repository.Add(company.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<CompanyDto>.Success(CompanyDto.From(company.Value));
    }
}
