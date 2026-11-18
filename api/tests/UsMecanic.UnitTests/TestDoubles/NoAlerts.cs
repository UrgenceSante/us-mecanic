using UsMecanic.Application.Vehicles;

namespace UsMecanic.UnitTests.TestDoubles;

/// <summary>Calcul d'alertes branché sur des dépôts vides, pour les tests de liste qui ne s'y intéressent pas.</summary>
internal static class NoAlerts
{
    public static VehicleAlertsLookup Lookup() => new(
        new InMemoryVehicleDeadlineRepository(),
        new InMemoryDeadlineTypeRepository(),
        new InMemoryDeadlineThresholdsRepository(),
        new InMemoryProblemRepository(),
        new FixedClock().Calendar);
}
