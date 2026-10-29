using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Analyse d'un problème : identifiants de la nature et de l'élément (référentiels), texte, immobilisation, date.</summary>
public sealed record ProblemAnalysisDto(Guid NatureId, Guid ElementId, string Text, bool Immobilized, DateTimeOffset AnalyzedAt)
{
    public static ProblemAnalysisDto? From(ProblemAnalysis? analysis) =>
        analysis is null ? null : new ProblemAnalysisDto(analysis.NatureId, analysis.ElementId, analysis.Text, analysis.Immobilized, analysis.AnalyzedAt);
}
