namespace MembershipSite.ViewModels;

public class EmailContacts
{
    /// <summary>
    /// Email address to BCC developer in on for logging / troubleshooting.
    /// </summary>
    public string DeveloperEmail { get; set; }

    /// <summary>
    /// List of email and name pairs address to send registration alerts to when people create accounts on the website.
    /// </summary>
    public List<EmailAndNamePair> RegistrationContacts { get; set; }

    /// <summary>
    /// Email address used as the sender for website emails.
    /// </summary>
    public string WebsiteFromEmail { get; set; }

    /// <summary>
    /// Name used as the sender for website emails.
    /// </summary>
    public string WebsiteFromName { get; set; }
}
