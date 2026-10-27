using Microsoft.EntityFrameworkCore;
using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Deadlines;

internal sealed class DeadlineThresholdsRepository(UsMecanicDbContext db) : IDeadlineThresholdsRepository
{
    public async Task<DeadlineThresholds> GetAsync(CancellationToken cancellationToken)
    {
        // La ligne est insérée par la migration ; on la recrée avec ses valeurs par défaut si elle a disparu.
        var thresholds = await db.GlobalDeadlineThresholds.FirstOrDefaultAsync(t => t.Id == DeadlineThresholds.SingletonId, cancellationToken);
        if (thresholds is not null)
        {
            return thresholds;
        }

        thresholds = DeadlineThresholds.CreateDefault();
        db.GlobalDeadlineThresholds.Add(thresholds);
        return thresholds;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
}
