namespace MembershipSite.ViewModels;

/// <summary>
/// An email and name pair loaded from configuration.
/// </summary>
public class EmailAndNamePair
{
    /// <summary>
    /// Email address for a contact listed in app settings.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Name of contact listed in app settings.
    /// </summary>
    public string Name { get; set; }
}
