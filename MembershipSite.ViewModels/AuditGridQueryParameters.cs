public class AuditGridQueryParameters
{
    public string? EmailFilter { get; set; }

    public DateTime? EndDate { get; set; }

    public string? EventNameFilter { get; set; }

    public int Page { get; set; } = 1;

    public string? PayloadFilter { get; set; }

    public int Size { get; set; } = 10;

    public string? SortDirection { get; set; }

    public string? SortField { get; set; }

    public DateTime? StartDate { get; set; }

    public bool? SuccessFilter { get; set; }
}
