using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Costs.Loans;

/// <summary>Liste les emprunts d'un véhicule (consultable même hors parc).</summary>
public sealed class ListLoans(MileageVehicleGuard guard, ILoanRepository loans)
{
    public async Task<Result<IReadOnlyList<LoanDto>>> ExecuteAsync(Guid vehicleId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<LoanDto>>.Failure(error);
        }

        var list = await loans.ListByVehicleAsync(vehicleId, cancellationToken);
        return Result<IReadOnlyList<LoanDto>>.Success([.. list.Select(LoanDto.From)]);
    }
}
