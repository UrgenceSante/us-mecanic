using UsMecanic.Application.Costs.Expenses;
using UsMecanic.Application.Maintenance.Interventions;
using UsMecanic.Application.Mileage;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Costs;
using UsMecanic.Domain.Documents;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Documents;

/// <summary>
/// Joint une facture à une intervention du carnet ou à une dépense ponctuelle saisie à la main, du même véhicule
/// (sans catégorie, plusieurs possibles). Mêmes contrôles de fichier que pour un document du véhicule ; un véhicule
/// sorti du parc accepte l'ajout (facture tardive). Ni le nom ni le contenu ne sont journalisés.
/// </summary>
public sealed class AddInvoice(
    MileageVehicleGuard guard,
    IInterventionRepository interventions,
    IExpenseRepository expenses,
    IDocumentRepository documents,
    IDocumentStorage storage,
    TimeProvider clock)
{
    public async Task<Result<DocumentDto>> ForInterventionAsync(
        Guid vehicleId, Guid interventionId, string? label, string? fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<DocumentDto>.Failure(error);
        }

        if (await interventions.GetAsync(vehicleId, interventionId, cancellationToken) is null)
        {
            return Result<DocumentDto>.Failure(InterventionErrors.NotFound);
        }

        return await SaveAsync(vehicleId, interventionId, null, label, fileName, content, cancellationToken);
    }

    public async Task<Result<DocumentDto>> ForExpenseAsync(
        Guid vehicleId, Guid expenseId, string? label, string? fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        var error = await guard.CheckAsync(vehicleId, forWrite: false, cancellationToken);
        if (error is not null)
        {
            return Result<DocumentDto>.Failure(error);
        }

        var expense = await expenses.GetAsync(vehicleId, expenseId, cancellationToken);
        if (expense is null)
        {
            return Result<DocumentDto>.Failure(ExpenseErrors.NotFound);
        }

        // Dépense d'utilisation courante ou importée par un connecteur : pas de pièce jointe (spec, US6).
        if (expense.Family != ExpenseFamily.OneOff || expense.Source != ExpenseSource.Manual)
        {
            return Result<DocumentDto>.Failure(DocumentErrors.ExpenseNotAttachable);
        }

        return await SaveAsync(vehicleId, null, expenseId, label, fileName, content, cancellationToken);
    }

    private async Task<Result<DocumentDto>> SaveAsync(
        Guid vehicleId, Guid? interventionId, Guid? expenseId, string? label, string? fileName, ReadOnlyMemory<byte> content, CancellationToken cancellationToken)
    {
        var file = DocumentFileValidator.Validate(content.Span, fileName);
        if (file.IsFailure)
        {
            return Result<DocumentDto>.Failure(file.Error!);
        }

        var document = Document.CreateInvoice(vehicleId, interventionId, expenseId, label, file.Value, clock.GetUtcNow());
        if (document.IsFailure)
        {
            return Result<DocumentDto>.Failure(document.Error!);
        }

        // Les métadonnées et le contenu sont validés ensemble par SaveAsync (même unité de travail).
        documents.Add(document.Value);
        await storage.SaveAsync(document.Value.Id, content, cancellationToken);
        return Result<DocumentDto>.Success(DocumentDto.From(document.Value));
    }
}
