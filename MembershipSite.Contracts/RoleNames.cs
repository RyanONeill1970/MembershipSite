namespace MembershipSite.Contracts;

public static class RoleNames
{
    /// <summary>
    /// Administrator who looks after just the member data.
    /// </summary>
    public const string MemberAdmin = "MemberAdmin";

    /// <summary>
    /// Authorised public member. Only defined in this role when they are approved.
    /// </summary>
    public const string Member = "Member";
}
