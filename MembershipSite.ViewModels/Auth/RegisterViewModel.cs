namespace MembershipSite.ViewModels.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Please provide your email address.")]
    [DataType(DataType.EmailAddress)]
    [MaxLength(MemberFieldLimits.Email, ErrorMessage = "Emails can be a maximum of {0} characters.")]
    public string Email { get; set; }

    [MaxLength(MemberFieldLimits.MemberNumber, ErrorMessage = "Member numbers can be a maximum of {0} characters.")]
    public string? MemberNumber { get; set; } = "";

    [Required(ErrorMessage = "Please enter your name.")]
    [MaxLength(MemberFieldLimits.Name, ErrorMessage = "Names can be a maximum of {0} characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please choose a password.")]
    [MaxLength(MemberFieldLimits.PasswordHash, ErrorMessage = "Passwords should be a maximum of {0} characters.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
    public string PasswordCompare { get; set; }
  
    public string? RegistrationError { get; set; }
}
