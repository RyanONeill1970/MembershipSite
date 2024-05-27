namespace MembershipSite.Datalayer.Models;

public class Member
{
    public DateTime DateRegistered { get; set; }

    public string Email { get; set; } = "";

    public bool IsAdmin { get; set; }

    public bool IsApproved { get; set; }

    public string MemberNumber { get; set; } = "";

    public string Name { get; set; } = "";

    public string PasswordHash { get; set; } = "";
}
