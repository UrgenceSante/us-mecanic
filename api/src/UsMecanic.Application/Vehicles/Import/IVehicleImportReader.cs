using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Vehicles.Import;

/// <summary>
/// Port de lecture d'une source de véhicules à importer (fichier CSV aujourd'hui,
/// autre format ou système externe demain). Les valeurs restent brutes : la validation est faite par l'import.
/// </summary>
public interface IVehicleImportReader
{
    Task<Result<IReadOnlyList<VehicleImportRow>>> ReadAsync(Stream content, CancellationToken cancellationToken);
}
