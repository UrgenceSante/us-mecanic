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

    /// <summary>
    /// Société propriétaire courante (facultative, sans historique) : identifiant d'une <c>Company</c>.
    /// Une fois renseignée, elle peut être changée mais jamais vidée.
    /// </summary>
    public Guid? OwnerCompanyId { get; private set; }

    public VehicleStatus Status { get; private set; } = VehicleStatus.Active;

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public VehicleDetails Details => new(Brand, Model, FirstRegistrationDate, Vin, InternalNumber, TypeId, OwnerCompanyId);

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
            return AssignOwnerWhenRetired(details, now);
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

    /// <summary>
    /// Véhicule sorti du parc : seul le propriétaire peut être saisi, et seulement s'il est vide ;
    /// les autres champs doivent rester inchangés.
    /// </summary>
    private Result<Vehicle> AssignOwnerWhenRetired(VehicleDetails details, DateTimeOffset now)
    {
        var otherFieldsUnchanged = Normalize(details) with { OwnerCompanyId = OwnerCompanyId } == Details;
        if (OwnerCompanyId is not null || details.OwnerCompanyId is null || !otherFieldsUnchanged)
        {
            return Result<Vehicle>.Failure(VehicleErrors.Retired);
        }

        OwnerCompanyId = details.OwnerCompanyId;
        UpdatedAt = now;
        return Result<Vehicle>.Success(this);
    }

    private static VehicleDetails Normalize(VehicleDetails details) =>
        details with { Brand = Clean(details.Brand), Model = Clean(details.Model), InternalNumber = Clean(details.InternalNumber) };

    private Result<Vehicle> ApplyDetails(VehicleDetails details, DateOnly today, DateTimeOffset now)
    {
        if (details.FirstRegistrationDate > today)
        {
            return Result<Vehicle>.Failure(VehicleErrors.FirstRegistrationInFuture);
        }

        if (details.OwnerCompanyId is null && OwnerCompanyId is not null)
        {
            return Result<Vehicle>.Failure(VehicleErrors.OwnerCannotBeRemoved);
        }

        var cleaned = Normalize(details);
        if (new[] { cleaned.Brand, cleaned.Model, cleaned.InternalNumber }.Any(text => text?.Length > MaxTextLength))
        {
            return Result<Vehicle>.Failure(VehicleErrors.FieldTooLong);
        }

        if (Details == cleaned)
        {
            return Result<Vehicle>.Success(this);
        }

        Brand = cleaned.Brand;
        Model = cleaned.Model;
        InternalNumber = cleaned.InternalNumber;
        OwnerCompanyId = details.OwnerCompanyId;
        FirstRegistrationDate = details.FirstRegistrationDate;
        Vin = details.Vin;
        TypeId = details.TypeId;
        UpdatedAt = now;
        return Result<Vehicle>.Success(this);
    }

    private static string? Clean(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
