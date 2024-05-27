namespace MembershipSite.ViewModels.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage ="Please provide your email address.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Please provide your 4 digit member number.")]
    public string MemberNumber { get; set; }

    [Required(ErrorMessage = "Please enter your name.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Please choose a password.")]
    public string Password { get; set; }

    public string? RegistrationError { get; set; }
}
