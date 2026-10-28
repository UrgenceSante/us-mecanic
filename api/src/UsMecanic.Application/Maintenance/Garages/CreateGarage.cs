using UsMecanic.Domain.Common;
using UsMecanic.Domain.Maintenance;

namespace UsMecanic.Application.Maintenance.Garages;

/// <summary>Ajoute un garage. Le nom est obligatoire et unique (sans tenir compte de la casse).</summary>
public sealed class CreateGarage(IGarageRepository repository)
{
    public async Task<Result<GarageDto>> ExecuteAsync(string? name, CancellationToken cancellationToken)
    {
        var garage = Garage.Create(name);
        if (garage.IsFailure)
        {
            return Result<GarageDto>.Failure(garage.Error!);
        }

        if (await repository.NameExistsAsync(garage.Value.Name, null, cancellationToken))
        {
            return Result<GarageDto>.Failure(GarageErrors.NameAlreadyExists);
        }

        repository.Add(garage.Value);
        await repository.SaveChangesAsync(cancellationToken);
        return Result<GarageDto>.Success(GarageDto.From(garage.Value));
    }
}
