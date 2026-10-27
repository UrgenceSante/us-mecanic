using UsMecanic.Application.Deadlines;
using UsMecanic.Domain.Deadlines;
using UsMecanic.Infrastructure.Persistence;

namespace UsMecanic.Infrastructure.Deadlines;

internal sealed class DeadlineCompletionRepository(UsMecanicDbContext db) : IDeadlineCompletionRepository
{
    public void Add(DeadlineCompletion completion) => db.DeadlineCompletions.Add(completion);
}
