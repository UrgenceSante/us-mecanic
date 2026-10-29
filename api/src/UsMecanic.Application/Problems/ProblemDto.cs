using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Problème signalé sur un véhicule ; <see cref="Analysis"/> est renseignée à partir du statut « Analysé » et
/// <see cref="Actions"/> liste ses actions de résolution.
/// </summary>
public sealed record ProblemDto(
    Guid Id,
    Guid VehicleId,
    DateOnly Date,
    string Description,
    string Author,
    ProblemStatus Status,
    DateTimeOffset RecordedAt,
    ProblemAnalysisDto? Analysis = null,
    IReadOnlyList<ProblemActionDto>? Actions = null)
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
            ProblemAnalysisDto.From(problem.Analysis),
            [.. problem.Actions.Select(ProblemActionDto.From)]);
    }
}
