using UsMecanic.Domain.Mileage;

namespace UsMecanic.Application.Mileage;

/// <summary>Corps d'une requête d'ajout ou de modification d'un relevé.</summary>
/// <param name="Date">Date du relevé.</param>
/// <param name="Kilometers">Kilométrage au compteur (entier positif ou nul).</param>
/// <param name="Reason">Motif, obligatoire si la valeur est incohérente avec les relevés voisins.</param>
/// <param name="ConfirmHighMileage">À <c>true</c> pour confirmer un kilométrage moyen supérieur à 1 000 km/jour.</param>
public sealed record MileageReadingInput(DateOnly Date, int Kilometers, string? Reason = null, bool ConfirmHighMileage = false)
{
    public MileageReadingEntry ToEntry() => new(Date, Kilometers, Reason, ConfirmHighMileage);
}
