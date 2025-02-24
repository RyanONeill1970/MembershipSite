namespace MembershipSite.Contracts;

/// <summary>
/// Maximum field sizes for the AuditLog entity, used in validation and database configuration.
/// </summary>
public static class AuditLogFieldLimits
{
    public const int Email = MemberFieldLimits.Email;
    public const int EventName = 20;
}
