namespace MembershipSite.Logic.Models;

public class MemberCsvUploadResult
{
    public int Added { get; set; }

    public List<string> Errors { get; set; } = [];

    public int Failed { get; set; }

    public int Updated { get; set; }
}
