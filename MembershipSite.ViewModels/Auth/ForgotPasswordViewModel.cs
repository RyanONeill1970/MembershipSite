namespace MembershipSite.ViewModels.Auth;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    public string LoginError { get; set; } = "";
}
