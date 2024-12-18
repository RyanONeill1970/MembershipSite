namespace MembershipSite.Logic.Models;

public class MemberCsvRow
{
    [CsvHelper.Configuration.Attributes.Name("Email")]
    public string Email { get; set; }

    [CsvHelper.Configuration.Attributes.Name("MemberNumber")]
    public string MemberNumber { get; set; }

    [CsvHelper.Configuration.Attributes.Name("Name")]
    public string Name { get; set; }
}
