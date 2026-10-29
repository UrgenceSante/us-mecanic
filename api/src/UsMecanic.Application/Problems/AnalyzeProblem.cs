using UsMecanic.Application.Mileage;
using UsMecanic.Application.Problems.References;
using UsMecanic.Application.Vehicles;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Problems;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Problems;

/// <summary>
/// Analyse un problème signalé (nature, élément, texte, immobilisation) : il passe à « Analysé ». Si l'immobilisation
/// est cochée, le véhicule passe à « immobilisé » (règle de statut du véhicule), dans la même sauvegarde. Un véhicule
/// sorti du parc, une nature ou un élément inconnu ou désactivé, ou un problème déjà analysé ou résolu sont refusés.
/// Le retour en service du véhicule n'est pas géré ici.
/// </summary>
public sealed class AnalyzeProblem(
    IProblemRepository problems,
    IVehicleRepository vehicles,
    MileageVehicleGuard guard,
    IProblemReferenceRepository<ProblemNature> natures,
    IProblemReferenceRepository<ProblemElement> elements,
    TimeProvider clock)
{
    public async Task<Result<ProblemDto>> ExecuteAsync(Guid problemId, AnalyzeProblemInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var problem = await problems.GetByIdAsync(problemId, cancellationToken);
        if (problem is null)
        {
            return Result<ProblemDto>.Failure(ProblemErrors.NotFound);
        }

        var guardError = await guard.CheckAsync(problem.VehicleId, forWrite: true, cancellationToken);
        if (guardError is not null)
        {
            return Result<ProblemDto>.Failure(guardError);
        }

        var nature = await natures.GetByIdAsync(input.NatureId, cancellationToken);
        var natureError = nature is null ? ProblemErrors.NatureUnknown : nature.GetSelectionError();
        if (natureError is not null)
        {
            return Result<ProblemDto>.Failure(natureError);
        }

        var element = await elements.GetByIdAsync(input.ElementId, cancellationToken);
        var elementError = element is null ? ProblemErrors.ElementUnknown : element.GetSelectionError();
        if (elementError is not null)
        {
            return Result<ProblemDto>.Failure(elementError);
        }

        var now = clock.GetUtcNow();
        var analyzed = problem.Analyze(input.NatureId, input.ElementId, input.Analysis, input.Immobilized, now);
        if (analyzed.IsFailure)
        {
            return Result<ProblemDto>.Failure(analyzed.Error!);
        }

        if (input.Immobilized)
        {
            var vehicle = await vehicles.GetByIdAsync(problem.VehicleId, cancellationToken);
            var immobilized = vehicle!.ChangeStatus(VehicleStatus.Immobilized, now);
            if (immobilized.IsFailure)
            {
                return Result<ProblemDto>.Failure(immobilized.Error!);
            }
        }

        // Problème et véhicule partagent le même contexte de persistance : une seule sauvegarde, donc une transaction.
        await problems.SaveChangesAsync(cancellationToken);
        return Result<ProblemDto>.Success(ProblemDto.From(problem));
    }
}
