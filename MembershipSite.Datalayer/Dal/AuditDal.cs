namespace MembershipSite.Datalayer.Dal;

public class AuditLogDal(MembershipContext context) : BaseDal(context)
{
    public AuditLog Add()
    {
        var row = new AuditLog
        {
            EventOccurred = DateTimeOffset.UtcNow,
            Id = Guid.NewGuid()
        };

        context.AuditLogs.Add(row);

        return row;
    }

    public IQueryable<AuditLog> AllAsQueryable()
    {
        return context.AuditLogs.AsQueryable();
    }

    /// <summary>
    /// Deletes logs past 10000 rows or > 30 days old.
    /// </summary>
    public void SweepOldRecords()
    {
        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);

        // Delete records older than 30 days
        context.AuditLogs
            .Where(log => log.EventOccurred < thirtyDaysAgo)
            .ExecuteDelete();

        // Keep only the latest 10,000 records
        var logsToDelete = context.AuditLogs
            .OrderByDescending(log => log.EventOccurred)
            .Skip(10000)
            .ToList();

        context.AuditLogs.RemoveRange(logsToDelete);
    }
}
