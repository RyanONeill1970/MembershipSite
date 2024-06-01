namespace MembershipSite.Contracts;

/// <summary>
/// Maximum field sizes for the Member entity, used in validation and database configuration.
/// </summary>
public static class MemberFieldLimits
{
    public const int MemberNumber = 5;
    public const int Email = 100;
    public const int Name = 100;
}