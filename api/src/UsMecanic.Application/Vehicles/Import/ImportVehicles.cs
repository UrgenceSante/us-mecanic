using System.Globalization;
using UsMecanic.Domain.Common;
using UsMecanic.Domain.Vehicles;

namespace UsMecanic.Application.Vehicles.Import;

/// <summary>
/// Intègre un parc existant : chaque ligne crée le véhicule ou met à jour celui de même immatriculation.
/// Une cellule vide ne remplace pas une valeur déjà connue, ce qui permet de compléter le parc
/// à partir de plusieurs sources. Les lignes valides sont enregistrées même si d'autres sont en erreur ;
/// la simulation permet de vérifier le bilan avant.
/// </summary>
public sealed class ImportVehicles(IVehicleImportReader reader, IVehicleRepository repository, TimeProvider clock, BusinessCalendar calendar)
{
    private static readonly string[] DateFormats = ["dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd"];

    private sealed record ValidRow(int Line, LicensePlate Plate, VehicleDetails Details);

    public async Task<Result<VehicleImportReport>> ExecuteAsync(Stream content, bool dryRun, CancellationToken cancellationToken)
    {
        var read = await reader.ReadAsync(content, cancellationToken);
        if (read.IsFailure)
        {
            return Result<VehicleImportReport>.Failure(read.Error!);
        }

        var rows = read.Value;
        if (rows.Count == 0)
        {
            return Result<VehicleImportReport>.Failure(ImportErrors.Empty);
        }

        if (rows.Count > ImportErrors.MaxRows)
        {
            return Result<VehicleImportReport>.Failure(ImportErrors.TooManyRows);
        }

        var results = new List<VehicleImportLineResult>();
        var validRows = Validate(rows, results);

        var existing = (await repository.GetByPlatesAsync([.. validRows.Select(row => row.Plate)], cancellationToken))
            .ToDictionary(vehicle => vehicle.LicensePlate);
        var now = clock.GetUtcNow();
        results.AddRange(validRows.Select(row => Apply(row, existing.GetValueOrDefault(row.Plate), calendar.Today, now)));

        if (!dryRun && results.Exists(r => r.Outcome is ImportOutcome.Created or ImportOutcome.Updated))
        {
            await repository.SaveChangesAsync(cancellationToken);
        }

        return Result<VehicleImportReport>.Success(new VehicleImportReport(dryRun, [.. results.OrderBy(r => r.Line)]));
    }

    private static List<ValidRow> Validate(IReadOnlyList<VehicleImportRow> rows, List<VehicleImportLineResult> errors)
    {
        var valid = new List<ValidRow>();
        var seen = new HashSet<LicensePlate>();

        foreach (var row in rows)
        {
            var parsed = Parse(row);
            if (parsed.IsSuccess && !seen.Add(parsed.Value.Plate))
            {
                parsed = Result<ValidRow>.Failure(ImportErrors.DuplicatePlate);
            }

            if (parsed.IsSuccess)
            {
                valid.Add(parsed.Value);
            }
            else
            {
                errors.Add(Error(row.Line, row.LicensePlate, parsed.Error!));
            }
        }

        return valid;
    }

    private static Result<ValidRow> Parse(VehicleImportRow row)
    {
        var plate = LicensePlate.Create(row.LicensePlate);
        if (plate.IsFailure)
        {
            return Result<ValidRow>.Failure(plate.Error!);
        }

        DateOnly? firstRegistration = null;
        if (!string.IsNullOrWhiteSpace(row.FirstRegistrationDate))
        {
            if (!DateOnly.TryParseExact(row.FirstRegistrationDate.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return Result<ValidRow>.Failure(ImportErrors.InvalidDate);
            }

            firstRegistration = date;
        }

        var details = new VehicleDetailsInput(row.Brand, row.Model, firstRegistration, row.Vin, row.InternalNumber).ToDetails();
        return details.IsSuccess
            ? Result<ValidRow>.Success(new ValidRow(row.Line, plate.Value, details.Value))
            : Result<ValidRow>.Failure(details.Error!);
    }

    private VehicleImportLineResult Apply(ValidRow row, Vehicle? vehicle, DateOnly today, DateTimeOffset now)
    {
        if (vehicle is null)
        {
            var registered = Vehicle.Register(row.Plate, row.Details, today, now);
            if (registered.IsFailure)
            {
                return Error(row.Line, row.Plate.Value, registered.Error!);
            }

            repository.Add(registered.Value);
            return new VehicleImportLineResult(row.Line, row.Plate.Value, ImportOutcome.Created);
        }

        var previousUpdate = vehicle.UpdatedAt;
        var updated = vehicle.UpdateDetails(MergeWithExisting(row.Details, vehicle.Details), today, now);
        if (updated.IsFailure)
        {
            return Error(row.Line, row.Plate.Value, updated.Error!);
        }

        var outcome = vehicle.UpdatedAt == previousUpdate ? ImportOutcome.Unchanged : ImportOutcome.Updated;
        return new VehicleImportLineResult(row.Line, row.Plate.Value, outcome);
    }

    private static VehicleDetails MergeWithExisting(VehicleDetails imported, VehicleDetails current) => new(
        string.IsNullOrWhiteSpace(imported.Brand) ? current.Brand : imported.Brand,
        string.IsNullOrWhiteSpace(imported.Model) ? current.Model : imported.Model,
        imported.FirstRegistrationDate ?? current.FirstRegistrationDate,
        imported.Vin ?? current.Vin,
        string.IsNullOrWhiteSpace(imported.InternalNumber) ? current.InternalNumber : imported.InternalNumber,
        imported.TypeId ?? current.TypeId);

    private static VehicleImportLineResult Error(int line, string? plate, DomainError error) =>
        new(line, plate, ImportOutcome.Error, error.Code, error.Message);
}
