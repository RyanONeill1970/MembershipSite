namespace MembershipSite.ViewModels.Auth;

public class SetPasswordViewModel
{
    [Required(ErrorMessage = "Please choose a password.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string PasswordCompare { get; set; }

    public Guid PasswordResetToken { get; set; }
}
