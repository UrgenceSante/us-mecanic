using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

/// <summary>
/// Emprunt qui finance un véhicule. Une mensualité est due chaque mois, du mois de la date de début
/// jusqu'à la fin de la durée (mois futurs compris). Un remboursement anticipé ou une vente
/// se traduit par une modification de la durée.
/// </summary>
public sealed class Loan
{
    public const int MaxLenderLength = 200;

    public const int MaxDurationMonths = 600;

    /// <summary>Plafond d'un montant, cohérent avec la colonne numeric(12,2).</summary>
    public const decimal MaxAmount = 9_999_999_999.99m;

    private Loan(Guid id, Guid vehicleId, LoanEntry entry, DateTimeOffset recordedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        RecordedAt = recordedAt;
        Lender = entry.Lender!;
        Apply(entry);
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    public string Lender { get; private set; }

    public decimal BorrowedAmount { get; private set; }

    public decimal MonthlyPayment { get; private set; }

    public DateOnly StartDate { get; private set; }

    public int DurationMonths { get; private set; }

    /// <summary>Date de saisie de l'emprunt (inchangée par une modification).</summary>
    public DateTimeOffset RecordedAt { get; }

    public static Result<Loan> Create(Guid vehicleId, LoanEntry entry, DateTimeOffset now)
    {
        var validated = Validate(entry);
        return validated.IsSuccess
            ? Result<Loan>.Success(new Loan(Guid.CreateVersion7(now), vehicleId, validated.Value, now))
            : Result<Loan>.Failure(validated.Error!);
    }

    public Result<Loan> Update(LoanEntry entry)
    {
        var validated = Validate(entry);
        if (validated.IsFailure)
        {
            return Result<Loan>.Failure(validated.Error!);
        }

        Apply(validated.Value);
        return Result<Loan>.Success(this);
    }

    /// <summary>
    /// Échéances de l'emprunt : une par mois, à partir du mois de la date de début, pour toute la durée.
    /// Le mois est identifié par son premier jour.
    /// </summary>
    public IReadOnlyList<LoanInstallment> Installments()
    {
        var first = new DateOnly(StartDate.Year, StartDate.Month, 1);
        return [.. Enumerable.Range(0, DurationMonths).Select(i => new LoanInstallment(first.AddMonths(i), MonthlyPayment))];
    }

    private static Result<LoanEntry> Validate(LoanEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var lender = entry.Lender?.Trim();
        var error = CheckLender(lender)
            ?? CheckAmount(entry.BorrowedAmount, LoanErrors.BorrowedAmountNegative)
            ?? CheckAmount(entry.MonthlyPayment, LoanErrors.MonthlyPaymentNegative)
            ?? CheckSchedule(entry);

        return error is null
            ? Result<LoanEntry>.Success(entry with { Lender = lender })
            : Result<LoanEntry>.Failure(error);
    }

    private static DomainError? CheckLender(string? lender)
    {
        if (string.IsNullOrEmpty(lender))
        {
            return LoanErrors.LenderRequired;
        }

        return lender.Length > MaxLenderLength ? LoanErrors.LenderTooLong : null;
    }

    private static DomainError? CheckAmount(decimal amount, DomainError negative)
    {
        if (amount < 0)
        {
            return negative;
        }

        if (decimal.Round(amount, 2) != amount)
        {
            return LoanErrors.AmountPrecision;
        }

        return amount > MaxAmount ? LoanErrors.AmountTooLarge : null;
    }

    private static DomainError? CheckSchedule(LoanEntry entry)
    {
        if (entry.StartDate == default)
        {
            return LoanErrors.StartDateRequired;
        }

        if (entry.DurationMonths <= 0)
        {
            return LoanErrors.DurationInvalid;
        }

        return entry.DurationMonths > MaxDurationMonths ? LoanErrors.DurationTooLong : null;
    }

    private void Apply(LoanEntry entry)
    {
        Lender = entry.Lender!;
        BorrowedAmount = entry.BorrowedAmount;
        MonthlyPayment = entry.MonthlyPayment;
        StartDate = entry.StartDate;
        DurationMonths = entry.DurationMonths;
    }
}
