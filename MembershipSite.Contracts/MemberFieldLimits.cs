namespace MembershipSite.Contracts;

/// <summary>
/// Maximum field sizes for the Member entity, used in validation and database configuration.
/// </summary>
public static class MemberFieldLimits
{
    public const int Email = 100;
    public const int MemberNumber = 5;
    public const int Name = 100;
    /// <summary>
    /// Password field as entered in the UI. This is not the storage size, for that see <see cref="PasswordHash"/>.
    /// </summary>
    public const int Password = 100;
    public const int PasswordHash = Password + 900; // Way overboard, but ensure we have enough space for hashed passwords
}
