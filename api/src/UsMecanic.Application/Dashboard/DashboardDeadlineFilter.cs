using UsMecanic.Domain.Deadlines;

namespace UsMecanic.Application.Dashboard;

/// <summary>Filtres facultatifs du tableau de bord ; <paramref name="Status"/> vaut <c>Overdue</c> ou <c>DueSoon</c>.</summary>
/// <remarks><c>CompanyId</c> : véhicules dont la société est propriétaire ou utilisatrice du jour.</remarks>
public sealed record DashboardDeadlineFilter(Guid? VehicleId = null, Guid? VehicleTypeId = null, Guid? DeadlineTypeId = null, DeadlineStatus? Status = null, Guid? CompanyId = null);
