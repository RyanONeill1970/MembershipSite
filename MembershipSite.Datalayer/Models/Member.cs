namespace MembershipSite.Datalayer.Models;

public class Member
{
    public DateTimeOffset DateRegistered { get; set; }

    public string Email { get; set; } = "";

    public bool IsAdmin { get; set; }

    public bool IsApproved { get; set; }

    public required string MemberNumber { get; set; }

    public string Name { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public Guid? PasswordResetToken { get; set; }

    public DateTimeOffset? PasswordResetTokenExpiry { get; set; }
}
