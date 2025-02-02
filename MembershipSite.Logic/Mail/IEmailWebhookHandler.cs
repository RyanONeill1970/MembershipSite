namespace MembershipSite.Logic.Mail;

/// <summary>
/// Interface for events that should be handled by a transactional email provider's inbound webhooks.
/// 
/// Used to create a pluggable system where we can use different mail providers.
/// </summary>
public interface IEmailWebhookHandler
{
    Task HandleDeliveryReportAsync(string payload);

    Task HandleSpamReportAsync(string payload);
}
