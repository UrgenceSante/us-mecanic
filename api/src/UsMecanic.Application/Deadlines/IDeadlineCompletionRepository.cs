using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Port de persistance des réalisations d'échéance. L'ajout est enregistré avec le même
/// <c>SaveChangesAsync</c> que les échéances et les relevés (unité de travail partagée, donc une seule transaction).
/// </summary>
public interface IDeadlineCompletionRepository
{
    void Add(DeadlineCompletion completion);
}
