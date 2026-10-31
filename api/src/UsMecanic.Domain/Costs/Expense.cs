using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

/// <summary>
/// Dépense d'un véhicule, enregistrée à sa date : famille, catégorie (obligatoire pour une dépense ponctuelle),
/// montant en euros, libellé facultatif, source et date de saisie.
/// </summary>
public sealed class Expense
{
    public const int MaxLabelLength = 200;

    /// <summary>Nombre de décimales d'un montant (centimes).</summary>
    public const int AmountScale = 2;

    /// <summary>Plus grand montant stockable en numeric(12,2).</summary>
    public const decimal MaxAmount = 9_999_999_999.99m;

    // Les paramètres portent le nom des propriétés : c'est ce qui permet à EF Core de rematérialiser l'entité.
    private Expense(
        Guid id, Guid vehicleId, ExpenseFamily family, DateOnly date, Guid? categoryId, decimal amount, string? label, ExpenseSource source, DateTimeOffset recordedAt)
    {
        Id = id;
        VehicleId = vehicleId;
        Family = family;
        Date = date;
        CategoryId = categoryId;
        Amount = amount;
        Label = label;
        Source = source;
        RecordedAt = recordedAt;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    public ExpenseFamily Family { get; }

    /// <summary>Date de la dépense (et non de sa saisie).</summary>
    public DateOnly Date { get; private set; }

    /// <summary>Catégorie ; <c>null</c> possible uniquement pour la famille d'utilisation courante.</summary>
    public Guid? CategoryId { get; private set; }

    /// <summary>Montant en euros : positif ou nul, 2 décimales au plus.</summary>
    public decimal Amount { get; private set; }

    public string? Label { get; private set; }

    public ExpenseSource Source { get; }

    /// <summary>Date de saisie.</summary>
    public DateTimeOffset RecordedAt { get; }

    /// <summary>
    /// Crée une dépense saisie à la main : seule la famille ponctuelle est acceptée. L'existence et l'état de la
    /// catégorie sont contrôlés par le cas d'usage.
    /// </summary>
    public static Result<Expense> CreateManual(
        Guid vehicleId, ExpenseFamily family, DateOnly? date, Guid? categoryId, decimal? amount, string? label, DateTimeOffset now)
    {
        if (family != ExpenseFamily.OneOff)
        {
            return Result<Expense>.Failure(ExpenseErrors.FamilyNotManual);
        }

        var checkedFields = Check(date, categoryId, amount, label);
        if (checkedFields.IsFailure)
        {
            return Result<Expense>.Failure(checkedFields.Error!);
        }

        var fields = checkedFields.Value;
        return Result<Expense>.Success(new Expense(
            Guid.CreateVersion7(now), vehicleId, family, fields.Date, categoryId, fields.Amount, fields.Label, ExpenseSource.Manual, now));
    }

    /// <summary>Modifie une dépense manuelle (mêmes contrôles qu'à la création) ; la famille et la source ne changent pas.</summary>
    public Result<Expense> Update(DateOnly? date, Guid? categoryId, decimal? amount, string? label)
    {
        if (Source != ExpenseSource.Manual)
        {
            return Result<Expense>.Failure(ExpenseErrors.NotManual);
        }

        var checkedFields = Check(date, categoryId, amount, label);
        if (checkedFields.IsFailure)
        {
            return Result<Expense>.Failure(checkedFields.Error!);
        }

        var fields = checkedFields.Value;
        Date = fields.Date;
        CategoryId = categoryId;
        Amount = fields.Amount;
        Label = fields.Label;
        return Result<Expense>.Success(this);
    }

    private static Result<CheckedFields> Check(DateOnly? date, Guid? categoryId, decimal? amount, string? label)
    {
        if (date is not { } day)
        {
            return Result<CheckedFields>.Failure(ExpenseErrors.DateRequired);
        }

        if (categoryId is null)
        {
            return Result<CheckedFields>.Failure(ExpenseErrors.CategoryRequired);
        }

        if (amount is not { } value)
        {
            return Result<CheckedFields>.Failure(ExpenseErrors.AmountRequired);
        }

        if (value < 0)
        {
            return Result<CheckedFields>.Failure(ExpenseErrors.AmountNegative);
        }

        if (value > MaxAmount)
        {
            return Result<CheckedFields>.Failure(ExpenseErrors.AmountTooLarge);
        }

        if (decimal.Round(value, AmountScale) != value)
        {
            return Result<CheckedFields>.Failure(ExpenseErrors.AmountTooPrecise);
        }

        var cleaned = string.IsNullOrWhiteSpace(label) ? null : label.Trim();
        return cleaned?.Length > MaxLabelLength
            ? Result<CheckedFields>.Failure(ExpenseErrors.LabelTooLong)
            : Result<CheckedFields>.Success(new CheckedFields(day, value, cleaned));
    }

    private sealed record CheckedFields(DateOnly Date, decimal Amount, string? Label);
}
