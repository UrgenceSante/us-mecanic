namespace UsMecanic.Domain.Deadlines;

/// <summary>Situation d'une échéance non close par rapport à la date du jour et au km actuel.</summary>
public enum DeadlineStatus
{
    /// <summary>Ni dépassée, ni proche.</summary>
    UpToDate,

    /// <summary>Dans le seuil « proche » (en jours ou en kilomètres), sans être dépassée.</summary>
    DueSoon,

    /// <summary>Date passée ou km actuel supérieur ou égal au km d'échéance.</summary>
    Overdue,
}
