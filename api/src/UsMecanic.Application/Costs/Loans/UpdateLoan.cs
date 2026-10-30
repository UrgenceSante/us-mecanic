using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Loans;

/// <summary>Modifie un emprunt, notamment sa durée (remboursement anticipé ou vente).</summary>
public sealed class UpdateLoan(MileageVehicleGuard guard, ILoanRepository loans)
{
    public async Task<Result<LoanDto>> ExecuteAsync(Guid vehicleId, Guid loanId, LoanInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<LoanDto>.Failure(error);
        }

        var loan = await loans.GetAsync(vehicleId, loanId, cancellationToken);
        if (loan is null)
        {
            return Result<LoanDto>.Failure(LoanErrors.NotFound);
        }

        var updated = loan.Update(input.ToEntry());
        if (updated.IsFailure)
        {
            return Result<LoanDto>.Failure(updated.Error!);
        }

        await loans.SaveChangesAsync(cancellationToken);
        return Result<LoanDto>.Success(LoanDto.From(loan));
    }
}
