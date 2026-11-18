using UsMecanic.Domain.Common;
using UsMecanic.Domain.Companies;

namespace UsMecanic.Application.Companies;

/// <summary>
/// Désactive une société : elle n'est plus proposée mais reste affichée sur les véhicules et locations qui la citent.
/// Il n'existe volontairement aucune suppression de société.
/// </summary>
public sealed class DeactivateCompany(ICompanyRepository repository)
{
    public async Task<Result<CompanyDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await repository.GetByIdAsync(id, cancellationToken);
        if (company is null)
        {
            return Result<CompanyDto>.Failure(CompanyErrors.NotFound);
        }

        company.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<CompanyDto>.Success(CompanyDto.From(company));
    }
}
