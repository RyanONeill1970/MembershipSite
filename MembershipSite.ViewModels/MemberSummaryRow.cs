namespace MembershipSite.ViewModels;

public class MemberSummaryRow
{
    [JsonPropertyName("approveAndSendEmail")]
    public bool ApproveAndSendEmail { get; set; }

    [JsonPropertyName("dateRegistered")]
    public DateTimeOffset DateRegistered { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("isApproved")]
    public bool IsApproved { get; set; }

    [JsonPropertyName("isDirty")]
    public bool IsDirty { get; set; }

    [JsonPropertyName("memberNumber")]
    public string MemberNumber { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("pendingDelete")]
    public bool PendingDelete { get; set; }
}
