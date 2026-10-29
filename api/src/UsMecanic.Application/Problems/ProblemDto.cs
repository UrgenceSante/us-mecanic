using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Problème signalé sur un véhicule ; <see cref="Analysis"/> est renseignée à partir du statut « Analysé ».</summary>
public sealed record ProblemDto(
    Guid Id,
    Guid VehicleId,
    DateOnly Date,
    string Description,
    string Author,
    ProblemStatus Status,
    DateTimeOffset RecordedAt,
    ProblemAnalysisDto? Analysis = null)
{
    public static ProblemDto From(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new ProblemDto(
            problem.Id,
            problem.VehicleId,
            problem.Date,
            problem.Description,
            problem.Author,
            problem.Status,
            problem.RecordedAt,
            ProblemAnalysisDto.From(problem.Analysis));
    }
}
