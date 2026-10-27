namespace UsMecanic.Domain.Mileage;

/// <summary>Valeurs saisies pour un relevé kilométrique, avant contrôle de cohérence.</summary>
/// <param name="Date">Date du relevé (jour où le compteur a été lu).</param>
/// <param name="Kilometers">Kilométrage au compteur.</param>
/// <param name="Reason">Motif, obligatoire si la valeur est incohérente avec les relevés voisins.</param>
/// <param name="ConfirmHighMileage">Confirmation explicite d'un kilométrage moyen supérieur au plafond journalier.</param>
public sealed record MileageReadingEntry(DateOnly Date, int Kilometers, string? Reason = null, bool ConfirmHighMileage = false);
