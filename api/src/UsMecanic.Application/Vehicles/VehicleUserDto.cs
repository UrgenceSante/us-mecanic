using UsMecanic.Application.Companies;

namespace UsMecanic.Application.Vehicles;

/// <summary>D'où vient l'utilisateur d'un véhicule (règle R4 de la phase 5 bis).</summary>
public enum VehicleUserSource
{
    /// <summary>Ni locataire ni propriétaire : « non renseigné ».</summary>
    NotSet,

    /// <summary>Le propriétaire, faute de location qui couvre la date.</summary>
    Owner,

    /// <summary>Le locataire d'une location qui couvre la date.</summary>
    Tenant,
}

/// <summary>Utilisateur d'un véhicule à une date : société et origine (<c>Company</c> est null si non renseigné).</summary>
public sealed record VehicleUserDto(CompanyDto? Company, VehicleUserSource Source)
{
    public static VehicleUserDto NotSet { get; } = new(null, VehicleUserSource.NotSet);
}
