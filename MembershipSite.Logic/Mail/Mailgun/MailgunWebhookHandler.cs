﻿namespace MembershipSite.Logic.Mail.Mailgun;

/// <summary>
/// Handles Mailgun specific inbound webhooks relating to email delivery.
/// </summary>
/// <param name="emailConfig"></param>
/// <param name="logger">Only ever null when used in a unit test.</param>
public class MailgunWebhookHandler(EmailConfig emailConfig, ILogger<MailgunWebhookHandler>? logger) : IEmailWebhookHandler
{
    public Task HandleDeliveryReportAsync(string payload)
    {
        var request = Deserialise<MailgunRequest>(payload, nameof(HandleDeliveryReportAsync));

        if (request is null)
        {
            return Task.CompletedTask;
        }

        logger?.LogInformation("Mailgun delivery report: {Payload}", payload);
        AppLogging.Write($"MailgunDeliveryReport, {payload}");

        return Task.CompletedTask;
    }

    public Task HandleSpamReportAsync(string payload)
    {
        var request = Deserialise<MailgunRequest>(payload, nameof(HandleSpamReportAsync));

        if (request is null)
        {
            return Task.CompletedTask;
        }

        logger?.LogInformation("Mailgun spam report: {Payload}", payload);
        AppLogging.Write($"MailgunSpamReport, {payload}");

        return Task.CompletedTask;
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
