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
                Member '{model.Name}' with membership number '{model.MemberNumber}' is trying to register again but has not yet had their original request approved.

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

        await SignInAsync(member);

        if (member.IsAdmin)
        {
            return LoginResult.Administrator;
        }
        return LoginResult.Success;
    }

    private async Task SignInAsync(Datalayer.Models.Member member)
    {
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
            IsPersistent = true, // model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(60) // Set cookie expiration date
        };

        var httpContext = httpContextAccessor.HttpContext!;
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authenticationProperties);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordViewModel model)
    {
        var member = await memberDal.ByEmailAsync(model.Email);

        if (member is null)
        {
            // We don't return a 'not found' message this as it could be used by an attacker doing credential stuffing or checking for valid emails.
            return;
        }

        member.PasswordResetToken = Guid.NewGuid();
        var httpContext = httpContextAccessor.HttpContext!;
        var websiteUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";

        var subject = $"{appSettings.ApplicationName} - password reset request.";
        var body = $"""
            Hi {member.Name},

            You have requested a password reset. Please click the following link to reset your password:

            {websiteUrl}/auth/set-password/{member.PasswordResetToken}

            If you did not request this, please ignore this email.

            Thank you,

            The {appSettings.ApplicationName} team.
            """;
        var contacts = appSettings.EmailContacts;
        await emailProvider.SendAsync(member.Name, model.Email, contacts.WebsiteFromName, contacts.WebsiteFromEmail,
            subject, body, null, false, contacts.DeveloperEmail);

        await memberDal.CommitAsync();
    }

    public async Task LogoutAsync()
    {
        var httpContext = httpContextAccessor.HttpContext!;
        await httpContext.SignOutAsync();
    }

    public async Task<bool> SetPasswordAsync(SetPasswordViewModel model)
    {
        // TODO: Have an expiry on the password reset token. Maybe have it configurable in the app settings.
        var member = await memberDal.ByPasswordResetTokenAsync(model.PasswordResetToken);

        if (member is null)
        {
            // TODO: Should instrument this for the admin.
            return false;
        }

        member.PasswordResetToken = null;
        member.PasswordHash = PasswordValidator.HashPassword(model.Password);

        await memberDal.CommitAsync();

        await SignInAsync(member);

        return true;
    }
}
