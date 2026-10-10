using UsMecanic.Domain.Common;

namespace UsMecanic.Application.Vehicles.Import;

public static class ImportErrors
{
    public const int MaxRows = 5000;

    public static readonly DomainError Empty =
        new("import.empty", "Le fichier ne contient aucune ligne de véhicule.");

    public static readonly DomainError TooManyRows =
        new("import.too_many_rows", $"Le fichier dépasse la limite de {MaxRows} lignes.");

    public static readonly DomainError DuplicatePlate =
        new("import.duplicate_plate", "Immatriculation déjà présente plus haut dans le fichier.");

    public static readonly DomainError InvalidDate =
        new("import.invalid_date", "Date de mise en circulation invalide (formats acceptés : JJ/MM/AAAA ou AAAA-MM-JJ).");
}
