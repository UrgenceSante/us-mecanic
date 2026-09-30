namespace UsMecanic.Domain.Common;

/// <summary>Erreur métier identifiée par un code stable, exposable au front.</summary>
public sealed record DomainError(string Code, string Message);
