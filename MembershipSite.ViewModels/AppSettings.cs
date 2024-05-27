namespace MembershipSite.ViewModels;

public class AppSettings
{
    public EmailContacts EmailContacts { get; set; }

    public EmailConfig EmailConfig { get; set; }

    /// <summary>
    /// Text to be added to the end of the page title for all pages rendered
    /// by the membership aspect of the site (authorisation and admin pages).
    /// 
    /// Used when bookmarking, storing in browser history or for SEO.
    /// 
    /// Suggested text should be of the format;
    ///     " - XYZ society".
    /// </summary>
    public string TitleSuffix { get; set; } = "";
}
