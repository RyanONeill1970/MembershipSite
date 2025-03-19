namespace MembershipSite.Logic.Mail.Mailgun.Models;

/// <summary>
/// All incoming models from Mailgun should inherit from this so that they
/// can use the generic signature verification code.
/// </summary>
public record MailgunWebhookBase
{
    [JsonPropertyName("signature")]
    public required MailgunSignature Signature { get; set; }

    [JsonPropertyName("event-data")]
    public MailgunWebhookEventData EventData { get; set; }
}

public class MailgunWebhookEventData
{
    [JsonPropertyName("delivery-status")]
    public MailgunWebhookDeliveryStatus DeliveryStatus { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("log-level")]
    public string LogLevel { get; set; }

    [JsonPropertyName("event")]
    public string EventName { get; set; }

    [JsonPropertyName("envelope")]
    public Envelope Envelope { get; set; }

    [JsonPropertyName("flags")]
    public Flags Flags { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonPropertyName("recipient")]
    public string RecipientEmail { get; set; }

    [JsonPropertyName("recipient-domain")]
    public string RecipientDomain { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("timestamp")]
    public float Timestamp { get; set; }

    [JsonPropertyName("user-variables")]
    public Dictionary<string, string> UserVariables { get; set; }
}

public record MailgunWebhookDeliveryStatus
{
    [JsonPropertyName("attempt-no")]
    public int AttemptNumber { get; set; }

    [JsonPropertyName("certificate-verified")]
    public bool CertificateVerified { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("mx-host")]
    public string MxHost { get; set; }

    [JsonPropertyName("tls")]
    public bool Tls { get; set; }

    [JsonPropertyName("session-seconds")]
    public float SessionSeconds { get; set; }

    [JsonPropertyName("utf8")]
    public bool Utf8 { get; set; }
}

public class Envelope
{
    [JsonPropertyName("sender")]
    public string Sender { get; set; }

    [JsonPropertyName("sending-ip")]
    public string SendingIp { get; set; }

    [JsonPropertyName("targets")]
    public string Targets { get; set; }

    [JsonPropertyName("transport")]
    public string Transport { get; set; }
}

public class Flags
{
    [JsonPropertyName("is-authenticated")]
    public bool IsAuthenticated { get; set; }

    [JsonPropertyName("is-routed")]
    public bool IsRouted { get; set; }

    [JsonPropertyName("is-system-test")]
    public bool IsSystemTest { get; set; }

    [JsonPropertyName("is-test-mode")]
    public bool IsTestMode { get; set; }
}

public class Message
{
    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}
