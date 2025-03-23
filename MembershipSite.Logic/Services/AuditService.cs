namespace MembershipSite.Logic.Services;

public class AuditService(AuditLogDal auditLogDal )
{
    public async Task<AuditGridSummary> AuditAdminSummaryAsync(AuditGridQueryParameters query)
    {
        var audits = auditLogDal.AllAsQueryable();

        audits = ApplyFiltering(query, audits);

        if (!string.IsNullOrEmpty(query.SortField))
        {
            audits = ApplySorting(audits, query.SortField, query.SortDirection);
        }
        else
        {
            // Default sort by event date descending
            audits = audits.OrderByDescending(a => a.EventOccurred);
        }

        var totalItems = await audits.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)query.Size);

        var paginatedData = await audits
            .Skip((query.Page - 1) * query.Size)
            .Take(query.Size)
            .Select(a => new AuditSummaryRow
            {
                Email = a.Email,
                EventName = a.EventName,
                EventOccurred = a.EventOccurred,
                Payload = a.Payload,
                Success = a.Success,
            })
            .ToListAsync();

        return new AuditGridSummary
        {
            CurrentPage = query.Page,
            Data = paginatedData,
            LastPage = totalPages,
            TotalItems = totalItems
        };
    }

    private static IQueryable<AuditLog> ApplyFiltering(AuditGridQueryParameters query, IQueryable<AuditLog> audits)
    {
        if (!string.IsNullOrEmpty(query.EmailFilter))
        {
            audits = audits.Where(a => a.Email.Contains(query.EmailFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(query.EventNameFilter))
        {
            audits = audits.Where(a => a.EventName.Contains(query.EventNameFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (query.SuccessFilter.HasValue)
        {
            audits = audits.Where(a => a.Success == query.SuccessFilter.Value);
        }

        if (!string.IsNullOrEmpty(query.PayloadFilter))
        {
            audits = audits.Where(a => a.Payload != null && a.Payload.Contains(query.PayloadFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (query.StartDate.HasValue)
        {
            audits = audits.Where(a => a.EventOccurred >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            audits = audits.Where(a => a.EventOccurred <= query.EndDate.Value);
        }

        return audits;
    }

    private IQueryable<AuditLog> ApplySorting(IQueryable<AuditLog> audits, string sortField, string? sortDirection)
    {
        var isAscending = string.IsNullOrEmpty(sortDirection) || sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);

        return sortField.ToLower() switch
        {
            "email" => isAscending
                ? audits.OrderBy(a => a.Email)
                : audits.OrderByDescending(a => a.Email),
            "eventoccurred" => isAscending
                ? audits.OrderBy(a => a.EventOccurred)
                : audits.OrderByDescending(a => a.EventOccurred),
            "success" => isAscending
                ? audits.OrderBy(a => a.Success)
                : audits.OrderByDescending(a => a.Success),
            "eventname" => isAscending
                ? audits.OrderBy(a => a.EventName)
                : audits.OrderByDescending(a => a.EventName),
            "payload" => isAscending
                ? audits.OrderBy(a => a.Payload)
                : audits.OrderByDescending(a => a.Payload),
            _ => isAscending
                ? audits.OrderBy(a => a.EventOccurred)
                : audits.OrderByDescending(a => a.EventOccurred)
        };
    }
}
