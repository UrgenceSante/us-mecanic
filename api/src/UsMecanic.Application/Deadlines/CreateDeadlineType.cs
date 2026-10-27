using UsMecanic.Domain.Common;
using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Deadlines;

/// <summary>Ajoute un type d'échéance. Le nom est obligatoire et unique (sans tenir compte de la casse).</summary>
public sealed class CreateDeadlineType(IDeadlineTypeRepository repository)
{
    public async Task<Result<DeadlineTypeDto>> ExecuteAsync(DeadlineTypeInput input, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);

        var type = DeadlineType.Create(input.Name, input.Settings);
        if (type.IsFailure)
        {
            return Result<DeadlineTypeDto>.Failure(type.Error!);
        }

        if (await repository.NameExistsAsync(type.Value.Name, null, cancellationToken))
        {
            return Result<DeadlineTypeDto>.Failure(DeadlineTypeErrors.NameAlreadyExists);
        }

        repository.Add(type.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<DeadlineTypeDto>.Success(DeadlineTypeDto.From(type.Value));
    }
}
