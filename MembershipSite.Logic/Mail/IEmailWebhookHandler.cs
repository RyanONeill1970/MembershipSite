namespace MembershipSite.Logic.Mail;

/// <summary>
/// Interface for events that should be handled by a transactional email provider's inbound webhooks.
/// 
/// Used to create a pluggable system where we can use different mail providers.
/// </summary>
public interface IEmailWebhookHandler
{
    Task HandleAcceptedAsync(string payload);

    Task HandleDeliveryReportAsync(string payload);

    Task HandlePermanentFailureAsync(string payload);

    Task HandleSpamReportAsync(string payload);

    Task HandleTemporaryFailureAsync(string payload);

    Task HandleUnsubscribeAsync(string payload);
}
