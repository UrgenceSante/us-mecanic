using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Problems;

/// <summary>Contrat statique d'un référentiel des problèmes : sa description et sa fabrique.</summary>
#pragma warning disable CA1000 // Membres statiques abstraits volontaires : la fabrique et la description se résolvent par type
public interface IProblemReference<TSelf>
    where TSelf : ProblemReference, IProblemReference<TSelf>
{
    static abstract ReferenceKind Definition { get; }

    static abstract Result<TSelf> Create(string? name);
}
#pragma warning restore CA1000
