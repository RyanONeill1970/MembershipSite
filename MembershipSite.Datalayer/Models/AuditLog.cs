namespace MembershipSite.Datalayer.Models;

public class AuditLog
{
    public string Email { get; set; } = "";

    public string EventName { get; set; } = "";

    public DateTimeOffset EventOccurred { get; set; }

    public Guid Id { get; set; }

    public string Payload { get; set; } = "";

    public bool Success { get; set; }
}
