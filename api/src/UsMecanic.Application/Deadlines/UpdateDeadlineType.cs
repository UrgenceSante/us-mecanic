using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Modifie un type d'échéance (nom, périodicité, seuils). Le nom doit rester unique.</summary>
public sealed class UpdateDeadlineType(IDeadlineTypeRepository repository)
{
    public async Task<Result<DeadlineTypeDto>> ExecuteAsync(Guid id, DeadlineTypeInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var validated = DeadlineType.Validate(input.Name, input.Settings);
        if (validated.IsFailure)
        {
            return Result<DeadlineTypeDto>.Failure(validated.Error!);
        }

        var type = await repository.GetByIdAsync(id, cancellationToken);
        if (type is null)
        {
            return Result<DeadlineTypeDto>.Failure(DeadlineTypeErrors.NotFound);
        }

        if (await repository.NameExistsAsync(validated.Value, id, cancellationToken))
        {
            return Result<DeadlineTypeDto>.Failure(DeadlineTypeErrors.NameAlreadyExists);
        }

        type.Update(validated.Value, input.Settings);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<DeadlineTypeDto>.Success(DeadlineTypeDto.From(type));
    }
}
