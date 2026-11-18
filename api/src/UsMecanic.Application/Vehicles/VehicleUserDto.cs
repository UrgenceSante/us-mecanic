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

    /// <summary>
    /// Règle R4 : le locataire, sinon le propriétaire, sinon non renseigné.
    /// Le locataire sera fourni par les locations (issue #209) ; il vaut null d'ici là.
    /// </summary>
    public static VehicleUserDto Resolve(CompanyDto? owner, CompanyDto? tenant) =>
        tenant is not null ? new(tenant, VehicleUserSource.Tenant)
        : owner is not null ? new(owner, VehicleUserSource.Owner)
        : NotSet;
}
