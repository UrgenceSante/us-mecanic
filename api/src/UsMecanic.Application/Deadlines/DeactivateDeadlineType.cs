using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>
/// Désactive un type : il n'est plus proposé mais reste affiché sur les échéances qui l'utilisent.
/// Il n'existe volontairement aucune suppression de type.
/// </summary>
public sealed class DeactivateDeadlineType(IDeadlineTypeRepository repository)
{
    public async Task<Result<DeadlineTypeDto>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var type = await repository.GetByIdAsync(id, cancellationToken);
        if (type is null)
        {
            return Result<DeadlineTypeDto>.Failure(DeadlineTypeErrors.NotFound);
        }

        type.Deactivate();
        await repository.SaveChangesAsync(cancellationToken);
        return Result<DeadlineTypeDto>.Success(DeadlineTypeDto.From(type));
    }
}
