namespace MembershipSite.Datalayer.Models;

public class Member
{
    public DateTimeOffset DateRegistered { get; set; }

    public required string Email { get; set; } = "";

    public DateTimeOffset? EmailLastFailed { get; set; }

    public DateTimeOffset? EmailLastSucceeded { get; set; }

    public bool IsAdmin { get; set; }

    public bool IsApproved { get; set; }

    public string MemberNumber { get; set; } = "";

    public string Name { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public Guid? PasswordResetToken { get; set; }

    public DateTimeOffset? PasswordResetTokenExpiry { get; set; }
}
