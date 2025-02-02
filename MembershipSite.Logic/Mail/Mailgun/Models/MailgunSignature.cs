namespace MembershipSite.Logic.Mail.Mailgun.Models;

public record MailgunSignature
{
    public string Signature { get; set; }

    public string Timestamp { get; set; }

    public string Token { get; set; }
}
