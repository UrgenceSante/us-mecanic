using System.Globalization;
using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Rentals;

public static class RentalErrors
{
    public static readonly DomainError NotFound =
        DomainError.NotFound("rental.not_found", "Location introuvable.");

    public static readonly DomainError TenantRequired =
        new("rental.tenant_required", "La société locataire est obligatoire.");

    public static readonly DomainError StartDateRequired =
        new("rental.start_date_required", "La date de début est obligatoire.");

    public static readonly DomainError EndBeforeStart =
        new("rental.end_before_start", "La date de fin doit être postérieure ou égale à la date de début.");

    public static readonly DomainError OwnerRequired =
        new("rental.owner_required", "Renseignez d'abord le propriétaire du véhicule.");

    public static readonly DomainError TenantIsOwner =
        new("rental.tenant_is_owner", "Le locataire ne peut pas être le propriétaire.");

    public static readonly DomainError VehicleRetired =
        DomainError.Conflict("rental.vehicle_retired", "Véhicule sorti du parc : aucune nouvelle location.");

    public static readonly DomainError AlreadyEnded =
        DomainError.Conflict("rental.already_ended", "Cette location a déjà une date de fin : modifiez-la plutôt.");

    /// <summary>Le nouveau propriétaire loue déjà le véhicule (location en cours ou à venir).</summary>
    public static DomainError OwnerIsTenant(string companyName) =>
        new("vehicle.owner_is_tenant", $"{companyName} loue ce véhicule : terminez ou supprimez d'abord cette location.");

    /// <summary>La période chevauche une autre location du véhicule, nommée dans le message.</summary>
    public static DomainError Overlap(string tenantName, DateOnly startDate, DateOnly? endDate) =>
        DomainError.Conflict(
            "rental.overlap",
            endDate is { } lastDay
                ? $"Chevauche la location à {tenantName} du {Format(startDate)} au {Format(lastDay)}"
                : $"Chevauche la location à {tenantName} depuis le {Format(startDate)}, sans date de fin");

    private static string Format(DateOnly day) => day.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
}
