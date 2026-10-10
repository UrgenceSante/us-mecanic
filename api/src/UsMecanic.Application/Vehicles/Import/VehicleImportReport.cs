namespace UsMecanic.Application.Vehicles.Import;

public enum ImportOutcome
{
    Created,
    Updated,
    Unchanged,
    Error,
}

public sealed record VehicleImportLineResult(
    int Line,
    string? LicensePlate,
    ImportOutcome Outcome,
    string? ErrorCode = null,
    string? Message = null);

/// <summary>Bilan d'un import. En simulation (<see cref="DryRun"/>), rien n'est enregistré.</summary>
public sealed record VehicleImportReport(bool DryRun, IReadOnlyList<VehicleImportLineResult> Lines)
{
    public int Created => Count(ImportOutcome.Created);

    public int Updated => Count(ImportOutcome.Updated);

    public int Unchanged => Count(ImportOutcome.Unchanged);

    public int Errors => Count(ImportOutcome.Error);

    private int Count(ImportOutcome outcome) => Lines.Count(line => line.Outcome == outcome);
}
