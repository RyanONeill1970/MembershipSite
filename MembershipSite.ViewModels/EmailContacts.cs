namespace MembershipSite.ViewModels;

public class EmailContacts
{
    /// <summary>
    /// Email address to BCC developer in on for logging / troubleshooting.
    /// </summary>
    public string DeveloperEmail { get; set; }

    /// <summary>
    /// Email address to send registration emails to when people create accounts on the website.
    /// </summary>
    public string RegistrationsToEmail { get; set; }

    /// <summary>
    /// Name to send registration emails to when people create accounts on the website.
    /// </summary>
    public string RegistrationsToName { get; set; }

    /// <summary>
    /// Email address used as the sender for website emails.
    /// </summary>
    public string WebsiteFromEmail { get; set; }

    /// <summary>
    /// Name used as the sender for website emails.
    /// </summary>
    public string WebsiteFromName { get; set; }
}
