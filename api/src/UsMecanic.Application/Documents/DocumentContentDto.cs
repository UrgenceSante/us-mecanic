namespace UsMecanic.Application.Documents;

/// <summary>Contenu d'un document avec ce qu'il faut pour le servir (type réel, nom d'origine).</summary>
public sealed record DocumentContentDto(string FileName, string ContentType, byte[] Content);
