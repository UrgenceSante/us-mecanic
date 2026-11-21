using System.Globalization;
using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Domain.Documents;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Ajoute à la liste des documents d'un véhicule le rattachement de ses factures (« Facture — intervention du
/// JJ/MM/AAAA »). Les dates se lisent en deux requêtes au plus, seulement si des factures existent.
/// </summary>
public sealed class InvoiceAttachmentLabeler(IInterventionRepository interventions, IExpenseRepository expenses)
{
    public async Task<IReadOnlyList<DocumentDto>> LabelAsync(
        Guid vehicleId, IReadOnlyList<Document> documents, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var interventionDates = documents.Any(d => d.InterventionId is not null)
            ? (await interventions.ListByVehicleAsync(vehicleId, cancellationToken)).ToDictionary(i => i.Id, i => i.Date)
            : [];
        var expenseDates = documents.Any(d => d.ExpenseId is not null)
            ? (await expenses.ListByVehicleAsync(vehicleId, cancellationToken)).ToDictionary(e => e.Id, e => e.Date)
            : [];

        return [.. documents.Select(document =>
        {
            var dto = DocumentDto.From(document);
            if (document.InterventionId is { } interventionId && interventionDates.TryGetValue(interventionId, out var interventionDate))
            {
                return dto with { AttachmentLabel = Label("intervention", interventionDate), AttachmentDate = interventionDate };
            }

            return document.ExpenseId is { } expenseId && expenseDates.TryGetValue(expenseId, out var expenseDate)
                ? dto with { AttachmentLabel = Label("dépense", expenseDate), AttachmentDate = expenseDate }
                : dto;
        })];
    }

    private static string Label(string kind, DateOnly date) =>
        $"Facture — {kind} du {date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}";
}
