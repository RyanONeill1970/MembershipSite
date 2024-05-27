namespace MembershipSite.ViewModels.Auth;

public enum RegisterUserResult
{
    RegisteredPendingApproval = 0,

    AlreadyExistsAsAuthorised = 1,

    AlreadyExistsAsPending = 2,
}
