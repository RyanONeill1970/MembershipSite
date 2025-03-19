namespace MembershipSite.Logic.Mail.Mailgun.Models;

public record MailgunSpamReport : MailgunWebhookBase
{
}
public record MailgunDeliveryReport : MailgunWebhookBase
{
}
public record MailgunAcceptReport : MailgunWebhookBase
{
}
public record MailgunPermanentFailureReport : MailgunWebhookBase
{
}
public record MailgunTemporaryFailureReport : MailgunWebhookBase
{
}
public record MailgunUnsubscribeReport : MailgunWebhookBase
{
}
