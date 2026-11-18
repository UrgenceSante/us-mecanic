using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Rentals;

/// <summary>
/// Location d'un véhicule à une société du groupe sur une période de jours entiers, bornes incluses
/// (une location du 01/02 au 31/03 couvre le 31/03). Sans date de fin, la location n'a pas de terme connu.
/// Les règles qui dépendent du véhicule ou des autres locations (propriétaire, chevauchement) sont contrôlées par l'Application.
/// </summary>
public sealed class Rental
{
    private Rental(Guid id, Guid vehicleId, Guid tenantCompanyId, DateOnly startDate, DateOnly? endDate)
    {
        Id = id;
        VehicleId = vehicleId;
        TenantCompanyId = tenantCompanyId;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Guid Id { get; }

    public Guid VehicleId { get; }

    public Guid TenantCompanyId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }

    public static Result<Rental> Create(Guid vehicleId, RentalEntry entry)
    {
        var error = Validate(entry);
        return error is null
            ? Result<Rental>.Success(new Rental(Guid.CreateVersion7(), vehicleId, entry.TenantCompanyId, entry.StartDate, entry.EndDate))
            : Result<Rental>.Failure(error);
    }

    public Result<Rental> Update(RentalEntry entry)
    {
        var error = Validate(entry);
        if (error is not null)
        {
            return Result<Rental>.Failure(error);
        }

        TenantCompanyId = entry.TenantCompanyId;
        StartDate = entry.StartDate;
        EndDate = entry.EndDate;
        return Result<Rental>.Success(this);
    }

    /// <summary>La location couvre ce jour (bornes incluses).</summary>
    public bool Covers(DateOnly day) => StartDate <= day && (EndDate is null || day <= EndDate);

    /// <summary>La location est en cours ou à venir : elle n'est pas terminée à ce jour.</summary>
    public bool IsCurrentOrUpcoming(DateOnly today) => EndDate is null || today <= EndDate;

    public RentalState StateOn(DateOnly today) =>
        today < StartDate ? RentalState.Planned : IsCurrentOrUpcoming(today) ? RentalState.Ongoing : RentalState.Ended;

    /// <summary>La location partage au moins un jour avec la période (bornes incluses, fin absente = sans limite).</summary>
    public bool Overlaps(DateOnly firstDay, DateOnly? lastDay) =>
        (EndDate is null || firstDay <= EndDate) && (lastDay is null || StartDate <= lastDay);

    /// <summary>Contrôle les champs saisis (locataire, début, fin ≥ début) ; <c>null</c> si la saisie est valide.</summary>
    public static DomainError? Validate(RentalEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.TenantCompanyId == Guid.Empty)
        {
            return RentalErrors.TenantRequired;
        }

        if (entry.StartDate == default)
        {
            return RentalErrors.StartDateRequired;
        }

        return entry.EndDate is { } lastDay && lastDay < entry.StartDate ? RentalErrors.EndBeforeStart : null;
    }
}
