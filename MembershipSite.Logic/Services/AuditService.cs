namespace MembershipSite.Logic.Services;

public class AuditService(AuditLogDal auditLogDal )
{
    public async Task<List<AuditSummaryRow>> AuditAdminSummaryAsync()
    {
        var members = await auditLogDal.AllAsQueryable().ToListAsync();

        return members
            .Select(a => new AuditSummaryRow
            {
                Email = a.Email,
                EventName = a.EventName,
                EventOccurred = a.EventOccurred,
                Payload = a.Payload,
                Success = a.Success,
            })
            .OrderByDescending(a => a.EventOccurred)
            .ToList();
    }
}
