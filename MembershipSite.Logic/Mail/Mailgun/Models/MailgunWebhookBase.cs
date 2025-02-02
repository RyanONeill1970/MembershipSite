namespace MembershipSite.Logic.Mail.Mailgun.Models;

/// <summary>
/// All incoming models from Mailgun should inherit from this so that they
/// can use the generic signature verification code.
/// </summary>
public record MailgunWebhookBase
{
    public required MailgunSignature Signature { get; set; }
}
