using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Garages;

/// <summary>Renomme un garage. Le nouveau nom doit rester unique.</summary>
public sealed class RenameGarage(IGarageRepository repository)
{
    public async Task<Result<GarageDto>> ExecuteAsync(Guid id, string? name, CancellationToken cancellationToken)
    {
        var normalized = Garage.NormalizeName(name);
        if (normalized.IsFailure)
        {
            return Result<GarageDto>.Failure(normalized.Error!);
        }

        var garage = await repository.GetByIdAsync(id, cancellationToken);
        if (garage is null)
        {
            return Result<GarageDto>.Failure(GarageErrors.NotFound);
        }

        if (await repository.NameExistsAsync(normalized.Value, id, cancellationToken))
        {
            return Result<GarageDto>.Failure(GarageErrors.NameAlreadyExists);
        }

        garage.Rename(normalized.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<GarageDto>.Success(GarageDto.From(garage));
    }
}
