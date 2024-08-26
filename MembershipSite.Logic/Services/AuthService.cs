namespace MembershipSite.Logic.Services;

public class AuthService(AppSettings appSettings, IEmailProvider emailProvider, IHttpContextAccessor httpContextAccessor, MemberDal memberDal)
{
    public async Task<RegisterUserOutput> RegisterUserAsync(RegisterViewModel model)
    {
        var member = await memberDal.ByMembershipNumberAsync(model.MemberNumber);

        if (member is null)
        {
            // User does not exist - register as pending and email webmaster.
            member = memberDal.Add();

            member.DateRegistered = DateTime.Now;
            member.Email = model.Email;
            member.IsAdmin = false;
            member.IsApproved = false;
            member.MemberNumber = model.MemberNumber;
            member.Name = model.Name;
            member.PasswordHash = PasswordValidator.HashPassword(model.Password);

            await memberDal.CommitAsync();

            var body = $"""
                Member '{model.Name}' with membership number '{model.MemberNumber}' has registered on the website and needs approving.

                Their email address is '{model.Email}'.
                """;
            var contacts = appSettings.EmailContacts;
            await emailProvider.SendAsync(contacts.RegistrationsToName, contacts.RegistrationsToEmail, contacts.WebsiteFromName, contacts.WebsiteFromEmail, "Website membership registration", body, [], false, contacts.DeveloperEmail);

            return new RegisterUserOutput { Result = RegisterUserResult.RegisteredPendingApproval };
        }

        if (!member.IsApproved)
        {
            // User exists and is not yet authorised, redirect to a 'you have been registered, we'll get back to you'.
            var body = $"""
                Member '{model.Name}' with membership number '{model.MemberNumber}' is trying to register again but has not yet has their original request approved.

                There is no issue with this, they will have been shown the same 'you have been registered' message as before.

                Their email address is '{model.Email}'.
                """;
            var contacts = appSettings.EmailContacts;
            await emailProvider.SendAsync(contacts.RegistrationsToName, contacts.RegistrationsToEmail, contacts.WebsiteFromName, contacts.WebsiteFromEmail, "Duplicate website membership registration", body, null, false, contacts.DeveloperEmail);

            return new RegisterUserOutput { Result = RegisterUserResult.AlreadyExistsAsPending };
        }

        // User exists and is authorised, redirect to a 'you are already registered, login or reset your password'.
        return new RegisterUserOutput { Result = RegisterUserResult.AlreadyExistsAsAuthorised };
    }

    public async Task<LoginResult> AuthenticateAsync(LoginViewModel model)
    {
        var member = await memberDal.ByEmailAsync(model.Email);

        if (member is null)
        {
            return LoginResult.NotFound;
        }

        if (!member.IsApproved)
        {
            return LoginResult.NotApproved;
        }

        if (!PasswordValidator.ComparePassword(model.Password, member.PasswordHash))
        {
            return LoginResult.InvalidPassword;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, member.MemberNumber),
        };

        if (member.IsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, RoleNames.MemberAdmin));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = true, // model.RememberMe, // Or true if you want the cookie to be persistent
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(60) // Set cookie expiration date
        };

        var httpContext = httpContextAccessor.HttpContext!;
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);

        if (member.IsAdmin)
        {
            return LoginResult.Administrator;
        }
        return LoginResult.Success;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordViewModel model)
    {
        // TODO: Complete this and password-reset view.
        // Retrieve member.
        // If exists, set a GUID on their password reset field.
        // Save.
        // Email them.
    }

    public async Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        await httpContext.SignOutAsync();
    }
}