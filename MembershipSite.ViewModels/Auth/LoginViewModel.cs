namespace MembershipSite.ViewModels.Auth;

using Microsoft.AspNetCore.Mvc;

public class LoginViewModel
{
    [Required(ErrorMessage = "Please enter your email address.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }

    public string? LoginError { get; set; }

    [Required(ErrorMessage = "Please enter your password.")]
    public string Password { get; set; }

    [BindProperty(Name = "r", SupportsGet = true)]
    public string? ReturnUrl { get; set; }
}
