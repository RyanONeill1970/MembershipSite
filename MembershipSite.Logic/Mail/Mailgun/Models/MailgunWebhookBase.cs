namespace MembershipSite.Logic.Mail.Mailgun.Models;

/// <summary>
/// All incoming models from Mailgun should inherit from this so that they
/// can use the generic signature verification code.
/// </summary>
public record MailgunWebhookBase
{
    [JsonPropertyName("signature")]
    public required MailgunSignature Signature { get; set; }

    [JsonPropertyName("eventdata")]
    public MailgunWebhookEventData EventData { get; set; }
}

public class MailgunWebhookEventData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("timestamp")]
    public float Timestamp { get; set; }

    [JsonPropertyName("loglevel")]
    public string LogLevel { get; set; }

    [JsonPropertyName("event")]
    public string EventName { get; set; }

    [JsonPropertyName("envelope")]
    public Envelope Envelope { get; set; }

    [JsonPropertyName("flags")]
    public Flags Flags { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; }

    [JsonPropertyName("recipient")]
    public string Recipient { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }

    [JsonPropertyName("uservariables")]
    public Dictionary<string, string> UserVariables { get; set; }
}

public class Envelope
{
    [JsonPropertyName("sendingip")]
    public string SendingIp { get; set; }
}

public class Flags
{
    [JsonPropertyName("istestmode")]
    public bool IsTestMode { get; set; }
}

public class Message
{
    [JsonPropertyName("headers")]
    public Dictionary<string, string> Headers { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}
