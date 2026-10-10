namespace UsMecanic.Domain.Vehicles;

/// <summary>Situation d'un véhicule dans le parc.</summary>
public enum VehicleStatus
{
    /// <summary>En service.</summary>
    Active,

    /// <summary>Temporairement indisponible (panne, réparation, contrôle…).</summary>
    Immobilized,

    /// <summary>Sorti du parc (vendu, réformé). Statut définitif.</summary>
    Retired,
}
