using UsMecanic.Domain.Problems;

namespace UsMecanic.Application.Problems;

/// <summary>Problème signalé sur un véhicule.</summary>
public sealed record ProblemDto(
    Guid Id,
    Guid VehicleId,
    DateOnly Date,
    string Description,
    string Author,
    ProblemStatus Status,
    DateTimeOffset RecordedAt)
{
    public static ProblemDto From(Problem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new ProblemDto(
            problem.Id, problem.VehicleId, problem.Date, problem.Description, problem.Author, problem.Status, problem.RecordedAt);
    }
}
