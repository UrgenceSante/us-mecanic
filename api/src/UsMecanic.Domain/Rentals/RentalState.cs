namespace UsMecanic.Domain.Rentals;

/// <summary>État d'une location par rapport à la date du jour (règle R5 de la phase 5 bis).</summary>
public enum RentalState
{
    /// <summary>Le début est dans le futur.</summary>
    Planned,

    /// <summary>La location couvre le jour.</summary>
    Ongoing,

    /// <summary>La fin est passée.</summary>
    Ended,
}
