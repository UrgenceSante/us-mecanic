namespace UsMecanic.Application.Mileage;

/// <summary>Km actuel d'un véhicule : valeur et date de son relevé le plus récent.</summary>
public sealed record CurrentMileage(int Kilometers, DateOnly Date);
