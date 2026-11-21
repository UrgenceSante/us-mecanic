namespace UsMecanic.Domain.Documents;

/// <summary>Fichier déjà contrôlé : nom nettoyé, type reconnu par la signature, taille en octets.</summary>
public sealed record DocumentFile(string FileName, DocumentContentType ContentType, long SizeBytes);
