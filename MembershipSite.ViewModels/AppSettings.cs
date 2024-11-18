namespace MembershipSite.ViewModels;

public class AppSettings
{
    /// <summary>
    /// The name by which this application is known to the data protection system.
    /// </summary>
    public string ApplicationName { get; set; }

    public EmailContacts EmailContacts { get; set; }

    public EmailConfig EmailConfig { get; set; }

    /// <summary>
    /// The number of days after a password reset token is issued that it will expire.
    /// </summary>
    public int PasswordResetTokenExpiryDays { get; set; }

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
