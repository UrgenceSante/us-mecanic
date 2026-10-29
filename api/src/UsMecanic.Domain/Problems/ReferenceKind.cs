namespace UsMecanic.Domain.Problems;

/// <summary>
/// Décrit un référentiel des problèmes : préfixe des codes d'erreur (ex. <c>problem_nature</c>)
/// et libellé affiché dans les messages (ex. « Nature »).
/// </summary>
public sealed record ReferenceKind(string Code, string Label);
