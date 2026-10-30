using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

public static class LoanErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("loan.not_found", "Emprunt introuvable.");

    public static readonly DomainError LenderRequired =
        new("loan.lender_required", "L'organisme prêteur est obligatoire.");

    public static readonly DomainError LenderTooLong =
        new("loan.lender_too_long", $"L'organisme est limité à {Loan.MaxLenderLength} caractères.");

    public static readonly DomainError BorrowedAmountRequired =
        new("loan.borrowed_amount_required", "Le montant emprunté est obligatoire.");

    public static readonly DomainError MonthlyPaymentRequired =
        new("loan.monthly_payment_required", "La mensualité est obligatoire.");

    public static readonly DomainError BorrowedAmountNegative =
        new("loan.borrowed_amount_negative", "Le montant emprunté ne peut pas être négatif.");

    public static readonly DomainError MonthlyPaymentNegative =
        new("loan.monthly_payment_negative", "La mensualité ne peut pas être négative.");

    public static readonly DomainError AmountPrecision =
        new("loan.amount_precision", "Les montants sont en euros avec au plus deux décimales.");

    public static readonly DomainError AmountTooLarge =
        new("loan.amount_too_large", $"Les montants sont limités à {Loan.MaxAmount} euros.");

    public static readonly DomainError StartDateRequired =
        new("loan.start_date_required", "La date de début est obligatoire.");

    public static readonly DomainError DurationInvalid =
        new("loan.duration_invalid", "La durée doit être un nombre entier de mois strictement positif.");

    public static readonly DomainError DurationTooLong =
        new("loan.duration_too_long", $"La durée est limitée à {Loan.MaxDurationMonths} mois.");
}
