using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Dépôt en mémoire des réalisations : les ajouts ne sont visibles qu'après <see cref="Flush"/>.</summary>
internal sealed class InMemoryDeadlineCompletionRepository : IDeadlineCompletionRepository
{
    private readonly List<DeadlineCompletion> _pending = [];

    public List<DeadlineCompletion> Saved { get; } = [];

    public void Add(DeadlineCompletion completion) => _pending.Add(completion);

    /// <summary>Simule l'enregistrement de l'unité de travail partagée.</summary>
    public Task Flush()
    {
        Saved.AddRange(_pending);
        _pending.Clear();
        return Task.CompletedTask;
    }
}
