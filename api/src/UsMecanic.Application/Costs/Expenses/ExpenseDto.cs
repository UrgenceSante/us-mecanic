using UsMecanic.Domain.Costs;

namespace UsMecanic.Application.Costs.Expenses;

/// <summary>Dépense d'un véhicule, avec le nom de sa catégorie (<c>null</c> si elle n'en a pas ou si elle est introuvable).</summary>
public sealed record ExpenseDto(
    Guid Id,
    Guid VehicleId,
    DateOnly Date,
    ExpenseFamily Family,
    Guid? CategoryId,
    string? CategoryName,
    decimal Amount,
    string? Label,
    ExpenseSource Source,
    DateTimeOffset RecordedAt);
