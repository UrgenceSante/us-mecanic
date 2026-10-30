using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Loans;

public sealed record LoanDto(
    Guid Id,
    Guid VehicleId,
    string Lender,
    decimal BorrowedAmount,
    decimal MonthlyPayment,
    DateOnly StartDate,
    int DurationMonths,
    DateTimeOffset RecordedAt)
{
    public static LoanDto From(Loan loan)
    {
        ArgumentNullException.ThrowIfNull(loan);

        return new(
            loan.Id, loan.VehicleId, loan.Lender, loan.BorrowedAmount, loan.MonthlyPayment, loan.StartDate, loan.DurationMonths, loan.RecordedAt);
    }
}
