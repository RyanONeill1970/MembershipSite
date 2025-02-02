namespace MembershipSite.Logic.Mail.Mailgun.Models;

public record MailgunSignature
{
    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }
}
