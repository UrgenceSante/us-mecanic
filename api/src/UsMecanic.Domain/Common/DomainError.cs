namespace UsMecanic.Domain.Common;

/// <summary>Erreur métier identifiée par un code stable, exposable au front.</summary>
public sealed record DomainError(string Code, string Message, ErrorKind Kind = ErrorKind.Validation)
{
    public static DomainError NotFound(string code, string message) => new(code, message, ErrorKind.NotFound);

    public static DomainError Conflict(string code, string message) => new(code, message, ErrorKind.Conflict);
}
