namespace UsMecanic.Domain.Maintenance;

/// <summary>Lien entre une intervention et une échéance du véhicule qu'elle a réalisée.</summary>
public sealed class InterventionDeadline
{
    internal InterventionDeadline(Guid interventionId, Guid vehicleDeadlineId)
    {
        InterventionId = interventionId;
        VehicleDeadlineId = vehicleDeadlineId;
    }

    public Guid InterventionId { get; }

    public Guid VehicleDeadlineId { get; }
}
