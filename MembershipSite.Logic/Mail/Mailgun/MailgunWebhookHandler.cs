namespace MembershipSite.Logic.Mail.Mailgun;

/// <summary>
/// Handles Mailgun specific inbound webhooks relating to email delivery.
/// </summary>
/// <param name="emailConfig"></param>
/// <param name="logger">Only ever null when used in a unit test.</param>
public class MailgunWebhookHandler(AppSettings appSettings, AuditLogDal auditLogDal,
    EmailConfig emailConfig, IEmailProvider emailProvider, ILogger<MailgunWebhookHandler>? logger,
    MemberDal memberDal) : IEmailWebhookHandler
{
    public async Task HandleDeliveryReportAsync(string payload)
    {
        var request = Deserialise<MailgunDeliveryReport>(payload, nameof(HandleDeliveryReportAsync));

        if (request is null)
        {
            return;
        }

        var member = await memberDal.ByEmailAsync(request.EventData.Recipient);

        if (member is not null && member.EmailLastSucceeded is null && member.EmailLastFailed is not null)
        {
            // This is the first successful email after a failure, let membership know it went through.
            var contacts = appSettings.EmailContacts;

            if (contacts.RegistrationContacts is not null)
            {
                var body = $"Emails to {request.EventData.Recipient} are now being delivered again.";

                foreach (var registrationContact in contacts.RegistrationContacts)
                {
                    await emailProvider.SendAsync(registrationContact.Name, registrationContact.Email, contacts.WebsiteFromName, contacts.WebsiteFromEmail,
                        "Website membership registration", body, [], false, contacts.DeveloperEmail);
                }
            }

            member.EmailLastFailed = null;
            member.EmailLastSucceeded = DateTimeOffset.UtcNow;
        }

        var auditLog = auditLogDal.Add();
        auditLog.Email = request.EventData.Recipient;
        auditLog.EventName = "Delivery";
        auditLog.Payload = payload;
        auditLog.Success = true;

        auditLogDal.SweepOldRecords();

        await memberDal.CommitAsync();
    }

    public async Task HandleSpamReportAsync(string payload)
    {
        var request = Deserialise<MailgunSpamReport>(payload, nameof(HandleSpamReportAsync));

        if (request is null)
        {
            return;
        }

        var member = await memberDal.ByEmailAsync(request.EventData.Recipient);

        if (member is not null && member.EmailLastFailed is null)
        {
            // This is the first successful email after a failure, let membership know it went through.
            var contacts = appSettings.EmailContacts;

            if (contacts.RegistrationContacts is not null)
            {
                var body = $"""
                    Email to {request.EventData.Recipient} has not been delivered.
                    You will not receive any more non-delivery reports for this email address
                    until a successful delivery has been made.
                    """;

                foreach (var registrationContact in contacts.RegistrationContacts)
                {
                    await emailProvider.SendAsync(registrationContact.Name, registrationContact.Email, contacts.WebsiteFromName, contacts.WebsiteFromEmail,
                        "Website membership registration", body, [], false, contacts.DeveloperEmail);
                }
            }

            member.EmailLastFailed = DateTimeOffset.UtcNow;
            member.EmailLastSucceeded = null;
        }

        var auditLog = auditLogDal.Add();
        auditLog.Email = request.EventData.Recipient;
        auditLog.EventName = "NonDelivery";
        auditLog.Payload = payload;
        auditLog.Success = false;

        auditLogDal.SweepOldRecords();

        await memberDal.CommitAsync();
    }

    internal T? Deserialise<T>(string payload, string context) where T : MailgunWebhookBase
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            logger?.LogWarning("Null or empty Mailgun payload for {Context}.", context);
            return null;
        }

        try
        {
            var request = JsonSerializer.Deserialize<T>(payload);

            if (request is null)
            {
                logger?.LogWarning("Failed to deserialise Mailgun request for {Context}, Payload: {Payload}",
                    context, payload);
                return null;
            }

            if (!VerifySignature(request))
            {
                logger?.LogWarning("Invalid Mailgun signature for {Context}, Payload: {Payload}",
                    context, payload);
                return null;
            }

            if (request.EventData.Recipient is null)
            {
                logger?.LogWarning("Null recipient in Mailgun delivery report: {Payload}, {Request}", payload, request);
                AppLogging.Write($"Null recipient in Mailgun delivery report, {payload}, {request.ToJson()}");
                return null;
            }

            return request;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to deserialise Mailgun request for {Context}, Payload: {Payload}",
                context, payload);
            return null;
        }
    }

    private bool VerifySignature(MailgunWebhookBase request)
    {
        var token = request.Signature.Token;
        var timestamp = request.Signature.Timestamp;
        var signature = request.Signature.Signature;

        if (string.IsNullOrWhiteSpace(emailConfig.WebhookSigningKey))
        {
            logger?.LogWarning("Mailgun API key is missing from configuration.");
            return false;
        }

        var hash = new HMACSHA256(Encoding.UTF8.GetBytes(emailConfig.WebhookSigningKey));
        var data = $"{timestamp}{token}";
        var computedSignature = Convert.ToHexStringLower(hash.ComputeHash(Encoding.UTF8.GetBytes(data)));
        var isValid = signature == computedSignature;

        if (!isValid)
        {
            logger?.LogWarning("Mailgun signature verification failed. Expected: {Expected}, Computed: {Computed}",
                signature, computedSignature);
        }

        return isValid;
    }
}
