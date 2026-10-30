using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Loans;

/// <summary>Supprime un emprunt. Renvoie l'emprunt supprimé.</summary>
public sealed class DeleteLoan(MileageVehicleGuard guard, ILoanRepository loans)
{
    public async Task<Result<LoanDto>> ExecuteAsync(Guid vehicleId, Guid loanId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<LoanDto>.Failure(error);
        }

        var loan = await loans.GetAsync(vehicleId, loanId, cancellationToken);
        if (loan is null)
        {
            return Result<LoanDto>.Failure(LoanErrors.NotFound);
        }

        loans.Remove(loan);
        await loans.SaveChangesAsync(cancellationToken);
        return Result<LoanDto>.Success(LoanDto.From(loan));
    }
}
