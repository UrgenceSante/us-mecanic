namespace UsMecanic.Application.Problems;

/// <summary>Corps d'une requête « analyser un problème ».</summary>
/// <param name="NatureId">Nature du problème (référentiel actif).</param>
/// <param name="ElementId">Élément concerné (référentiel actif).</param>
/// <param name="Analysis">Texte de l'analyse (obligatoire).</param>
/// <param name="Immobilized">Coché : le véhicule passe au statut « immobilisé ».</param>
public sealed record AnalyzeProblemInput(Guid NatureId, Guid ElementId, string? Analysis, bool Immobilized);
