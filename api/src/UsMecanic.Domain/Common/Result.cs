namespace UsMecanic.Domain.Common;

/// <summary>
/// Résultat d'une opération métier : succès avec une valeur, ou échec avec une <see cref="DomainError"/>.
/// Évite l'usage d'exceptions pour les cas d'erreur attendus.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(DomainError error)
    {
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Impossible de lire la valeur d'un résultat en échec.");

#pragma warning disable CA1000 // Fabriques statiques volontaires sur le type générique
    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(DomainError error) => new(error);
#pragma warning restore CA1000
}
