namespace MembershipSite.ViewModels.Auth;

public enum LoginResult
{
    NotFound = 0,

    NotApproved = 1,

    Success = 2,

    InvalidPassword = 3,

    Administrator = 4,
}
