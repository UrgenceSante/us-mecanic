using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Loans;

/// <summary>Enregistre un emprunt pour un véhicule du parc.</summary>
public sealed class AddLoan(MileageVehicleGuard guard, ILoanRepository loans, TimeProvider clock)
{
    public async Task<Result<LoanDto>> ExecuteAsync(Guid vehicleId, LoanInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var error = await guard.CheckAsync(vehicleId, forWrite: true, cancellationToken);
        if (error is not null)
        {
            return Result<LoanDto>.Failure(error);
        }

        var created = Loan.Create(vehicleId, input.ToEntry(), clock.GetUtcNow());
        if (created.IsFailure)
        {
            return Result<LoanDto>.Failure(created.Error!);
        }

        loans.Add(created.Value);
        await loans.SaveChangesAsync(cancellationToken);
        return Result<LoanDto>.Success(LoanDto.From(created.Value));
    }
}
