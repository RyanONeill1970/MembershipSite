namespace MembershipSite.Website.Controllers;

[AllowAnonymous]
[Route("auth")]
public class AuthController(AppSettings appSettings, AuthService authService, ILogger<AuthController> logger) : Controller
{
    [ActionName("forgot-password")]
    [Route("forgot-password", Name = "forgot-password")]
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        var model = new ForgotPasswordViewModel();
        return View(model);
    }

    [ActionName("forgot-password")]
    [Route("forgot-password", Name = "forgot-password")]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            await authService.ForgotPasswordAsync(model);

            return RedirectToRoute(nameof(PasswordReset));
        }

        model.LoginError = "Unable to access your details. Please contact us.";
        return View(model);
    }

    [Route("login", Name = nameof(Login))]
    [HttpGet]
    public IActionResult Login()
    {
        var model = new LoginViewModel();
        return View(model);
    }

    [Route("login")]
    [HttpPost]
    [ValidateAntiForgeryToken()]
    public async Task<IActionResult> LoginAsync(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var loginResult = await authService.AuthenticateAsync(model);

            if (loginResult == LoginResult.Success)
            {
                if (string.IsNullOrWhiteSpace(model.ReturnUrl))
                {
                    return Redirect(appSettings.SecureAreaRoot);
                }

                return this.RedirectToLocal(model.ReturnUrl);
            }
            if (loginResult == LoginResult.Administrator)
            {
                if (string.IsNullOrWhiteSpace(model.ReturnUrl))
                {
                    return RedirectToRoute(nameof(BackstageController.MemberList));
                }

                return this.RedirectToLocal(model.ReturnUrl);
            }
        }

        model.LoginError = "Unable to log you in. Please check your details.";
        return View(model);
    }

    [Route("logout")]
    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        return View();
    }

    [Route("register", Name = "register")]
    [HttpGet]
    public IActionResult Register()
    {
        var model = new RegisterViewModel();
        return View(model);
    }

    [Route("register")]
    [HttpPost]
    [ValidateAntiForgeryToken()]
    public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var registerUserOutput = await authService.RegisterUserAsync(model);

            if (registerUserOutput.Result == RegisterUserResult.RegisteredPendingApproval ||
                registerUserOutput.Result == RegisterUserResult.AlreadyExistsAsPending)
            {
                return RedirectToRoute(nameof(PendingApproval));
            }

            if (registerUserOutput.Result == RegisterUserResult.AlreadyExistsAsAuthorised)
            {
                return RedirectToRoute(nameof(AlreadyAuthorised));
            }
        }

        // One of those 'should not happen' moments, so log for investigation.
        logger.LogWarning("Registration ModelState was {ModelStateJson}. Someone tried to bypass client validation?", JsonSerializer.Serialize(ModelState));
        model.RegistrationError = "Unable to register your account, please contact us.";
        return View(model);
    }

    [ActionName("pending-approval")]
    [Route("pending-approval", Name = nameof(PendingApproval))]
    [HttpGet]
    public IActionResult PendingApproval()
    {
        return View();
    }

    [ActionName("already-authorised")]
    [Route("already-authorised", Name = nameof(AlreadyAuthorised))]
    [HttpGet]
    public IActionResult AlreadyAuthorised()
    {
        return View();
    }

    [ActionName("access-denied")]
    [Route("access-denied", Name = nameof(AccessDenied))]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [ActionName("token-expired")]
    [Route("token-expired", Name = nameof(TokenExpired))]
    [HttpGet]
    public IActionResult TokenExpired()
    {
        return View();
    }

    [ActionName("password-reset")]
    [Route("password-reset", Name = nameof(PasswordReset))]
    [HttpGet]
    public IActionResult PasswordReset()
    {
        return View();
    }

    [ActionName("set-password")]
    [Route("set-password/{passwordResetToken:guid}", Name = "set-password")]
    [HttpGet]
    public async Task<IActionResult> SetPasswordAsync(Guid passwordResetToken)
    {
        if (ModelState.IsValid)
        {
            var tokenIsValid = await authService.ValidatePasswordResetToken(passwordResetToken);

            if (tokenIsValid)
            {
                var model = new SetPasswordViewModel { PasswordResetToken = passwordResetToken };
                return View(model);
            }

            return RedirectToRoute(nameof(TokenExpired));
        }

        // Someone is fiddling with the route parameters, just redirect to access denied.
        return RedirectToRoute(nameof(AccessDenied));
    }

    [ActionName("set-password")]
    [Route("set-password/{passwordResetToken:guid}", Name = "set-password")]
    [HttpPost]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var success = await authService.SetPasswordAsync(model);

            if (success)
            {
                return RedirectToRoute(nameof(PasswordHasBeenSet));
            }

            // TODO: Could do with instrumenting this. If end users see this, it's annoying and painful to debug.
            ModelState.AddModelError(nameof(SetPasswordViewModel.Password), "Unable to set your password. Please try again.");
        }

        return View(model);
    }

    [ActionName("password-has-been-set")]
    [Route("password-has-been-set", Name = nameof(PasswordHasBeenSet))]
    [HttpGet]
    public IActionResult PasswordHasBeenSet()
    {
        return View();
    }
}
