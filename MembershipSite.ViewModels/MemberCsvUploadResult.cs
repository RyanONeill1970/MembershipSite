namespace MembershipSite.ViewModels;

public class MemberCsvUploadResult
{
    public List<string> Added { get; set; } = [];

    public List<string> Detail { get; set; } = [];

    public List<string> Failed { get; set; } = [];

    public List<string> Updated { get; set; } = [];
}
