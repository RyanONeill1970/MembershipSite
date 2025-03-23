public record AuditGridSummary
{
    [JsonPropertyName("current_page")]
    public int CurrentPage;

    [JsonPropertyName("data")]
    public List<AuditSummaryRow> Data { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }

    [JsonPropertyName("total_items")]
    public int TotalItems { get; set; }
}