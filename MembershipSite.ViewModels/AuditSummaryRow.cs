namespace MembershipSite.ViewModels;

public record AuditSummaryRow
{
    public string Email { get; set; }

    public string EventName { get; set; }

    public DateTimeOffset EventOccurred { get; set; }

    public string Payload { get; set; }

    public bool Success { get; set; }
}
