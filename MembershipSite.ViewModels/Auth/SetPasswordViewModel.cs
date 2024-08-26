namespace MembershipSite.ViewModels.Auth;

using System;

public class SetPasswordViewModel
{
    [Required(ErrorMessage = "Please choose a password.")]
    public string Password { get; set; }

    public Guid PasswordResetToken { get; set; }
}
