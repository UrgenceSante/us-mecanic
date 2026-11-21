using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Documents;

/// <summary>Factures d'une intervention ou d'une dépense du véhicule (métadonnées seulement, du plus récent au plus ancien).</summary>
public sealed class ListInvoices(
    MileageVehicleGuard guard, IInterventionRepository interventions, IExpenseRepository expenses, IDocumentRepository documents)
{
    public async Task<Result<IReadOnlyList<DocumentDto>>> ForInterventionAsync(
        Guid vehicleId, Guid interventionId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(error);
        }

        if (await interventions.GetAsync(vehicleId, interventionId, cancellationToken) is null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(InterventionErrors.NotFound);
        }

        var list = await documents.ListByInterventionAsync(vehicleId, interventionId, cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success([.. list.Select(DocumentDto.From)]);
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> ForExpenseAsync(
        Guid vehicleId, Guid expenseId, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(error);
        }

        if (await expenses.GetAsync(vehicleId, expenseId, cancellationToken) is null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(ExpenseErrors.NotFound);
        }

        var list = await documents.ListByExpenseAsync(vehicleId, expenseId, cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success([.. list.Select(DocumentDto.From)]);
    }
}
