using UsMecanic.Domain.Common;

namespace UsMecanic.Domain.Costs;

public static class CostSummaryErrors
{
    /// <summary>Plus longue période demandée, en années (borne le nombre de lignes renvoyées).</summary>
    public const int MaxYears = 10;

    public static readonly DomainError FromRequired =
        new("cost_summary.from_required", "La date de début de la période est obligatoire.");

    public static readonly DomainError ToRequired =
        new("cost_summary.to_required", "La date de fin de la période est obligatoire.");

    public static readonly DomainError InvalidRange =
        new("cost_summary.invalid_range", "La date de début doit précéder ou égaler la date de fin.");

    public static readonly DomainError RangeTooLong =
        new("cost_summary.range_too_long", $"La période est limitée à {MaxYears} ans.");

    public static readonly DomainError InvalidGranularity =
        new("cost_summary.invalid_granularity", "La granularité doit valoir « month » ou « year ».");
}
