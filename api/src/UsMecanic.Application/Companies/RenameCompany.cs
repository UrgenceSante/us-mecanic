using UsMecanic.Domain.Common;
using UsMecanic.Domain.Companies;

namespace UsMecanic.Application.Companies;

/// <summary>Renomme une société. Le nouveau nom doit rester unique.</summary>
public sealed class RenameCompany(ICompanyRepository repository)
{
    public async Task<Result<CompanyDto>> ExecuteAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        var normalized = Company.NormalizeName(name);
        if (normalized.IsFailure)
        {
            return Result<CompanyDto>.Failure(normalized.Error!);
        }

        var company = await repository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return Result<CompanyDto>.Failure(CompanyErrors.NotFound);
        }

        if (await repository.NameExistsAsync(normalized.Value, id, cancellationToken))
        {
            return Result<CompanyDto>.Failure(CompanyErrors.NameAlreadyExists);
        }

        company.Rename(normalized.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<CompanyDto>.Success(CompanyDto.From(company));
    }
}
