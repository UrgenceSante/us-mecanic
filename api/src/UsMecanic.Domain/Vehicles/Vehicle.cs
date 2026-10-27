using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Vehicles;

/// <summary>
/// Véhicule du parc. L'immatriculation l'identifie fonctionnellement (unique) ;
/// <see cref="Id"/> reste stable même si la plaque change.
/// </summary>
public sealed class Vehicle
{
    public const int MaxTextLength = 100;

    private Vehicle(Guid id, LicensePlate licensePlate, DateTimeOffset createdAt)
    {
        Id = id;
        LicensePlate = licensePlate;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; }

    public LicensePlate LicensePlate { get; private set; }

    public string? Brand { get; private set; }

    public string? Model { get; private set; }

    public DateOnly? FirstRegistrationDate { get; private set; }

    public Vin? Vin { get; private set; }

    /// <summary>Numéro ou indicatif interne au parc (ex. « AMB-12 »).</summary>
    public string? InternalNumber { get; private set; }

    /// <summary>Type d'utilisation (facultatif) : identifiant d'un <see cref="VehicleType"/>.</summary>
    public Guid? TypeId { get; private set; }

    public VehicleStatus Status { get; private set; } = VehicleStatus.Active;

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public VehicleDetails Details => new(Brand, Model, FirstRegistrationDate, Vin, InternalNumber, TypeId);

    public static Result<Vehicle> Register(LicensePlate licensePlate, VehicleDetails details, DateOnly today, DateTimeOffset now)
    {
        var vehicle = new Vehicle(Guid.CreateVersion7(now), licensePlate, now);
        var applied = vehicle.ApplyDetails(details, today, now);
        return applied.IsSuccess ? Result<Vehicle>.Success(vehicle) : Result<Vehicle>.Failure(applied.Error!);
    }

    public Result<Vehicle> UpdateDetails(VehicleDetails details, DateOnly today, DateTimeOffset now)
    {
        if (Status == VehicleStatus.Retired)
        {
            return Result<Vehicle>.Failure(VehicleErrors.Retired);
        }

        return ApplyDetails(details, today, now);
    }

    public Result<Vehicle> ChangeStatus(VehicleStatus status, DateTimeOffset now)
    {
        if (Status == VehicleStatus.Retired && status != VehicleStatus.Retired)
        {
            return Result<Vehicle>.Failure(VehicleErrors.Retired);
        }

        if (Status != status)
        {
            Status = status;
            UpdatedAt = now;
        }

        return Result<Vehicle>.Success(this);
    }

    private Result<Vehicle> ApplyDetails(VehicleDetails details, DateOnly today, DateTimeOffset now)
    {
        if (details.FirstRegistrationDate > today)
        {
            return Result<Vehicle>.Failure(VehicleErrors.FirstRegistrationInFuture);
        }

        var brand = Clean(details.Brand);
        var model = Clean(details.Model);
        var internalNumber = Clean(details.InternalNumber);
        if (new[] { brand, model, internalNumber }.Any(text => text?.Length > MaxTextLength))
        {
            return Result<Vehicle>.Failure(VehicleErrors.FieldTooLong);
        }

        if (Details == details with { Brand = brand, Model = model, InternalNumber = internalNumber })
        {
            return Result<Vehicle>.Success(this);
        }

        Brand = brand;
        Model = model;
        InternalNumber = internalNumber;
        FirstRegistrationDate = details.FirstRegistrationDate;
        Vin = details.Vin;
        TypeId = details.TypeId;
        UpdatedAt = now;
        return Result<Vehicle>.Success(this);
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
