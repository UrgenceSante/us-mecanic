namespace UsMecanic.Domain.Problems;

/// <summary>Analyse d'un problème : nature, élément concerné, texte, immobilisation du véhicule et date de l'analyse.</summary>
public sealed record ProblemAnalysis(Guid NatureId, Guid ElementId, string Text, bool Immobilized, DateTimeOffset AnalyzedAt);
