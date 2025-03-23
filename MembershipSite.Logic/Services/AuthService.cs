namespace MembershipSite.Logic.Services;

public class AuthService(AppSettings appSettings, AuditLogDal auditLogDal, IEmailProvider emailProvider, IHttpContextAccessor httpContextAccessor, MemberDal memberDal, TimeProvider timeProvider)
{
    public async Task<RegisterUserOutput> RegisterUserAsync(RegisterViewModel model)
    {
        var member = await memberDal.ByEmailAsync(model.Email);

        if (member is null)
        {
            // User does not exist - register as pending and email webmaster.
            member = memberDal.Add(model.Email);

            member.DateRegistered = DateTimeOffset.UtcNow;
            member.IsAdmin = false;
            member.IsApproved = false;
            member.MemberNumber = model.MemberNumber ?? string.Empty;
            member.Name = model.Name;
            member.PasswordHash = PasswordValidator.HashPassword(model.Password);

            await memberDal.CommitAsync();

            var body = $"""
                Member '{model.Name}' with membership number '{model.MemberNumber}' has registered on the website and needs approving.

                Their email address is '{model.Email}'.
                """;
            var contacts = appSettings.EmailContacts;

            if (contacts.RegistrationContacts is not null)
            {
                foreach (var registrationContact in contacts.RegistrationContacts)
                {
                    await emailProvider.SendAsync(registrationContact.Name, registrationContact.Email, contacts.WebsiteFromName, contacts.WebsiteFromEmail, "Website membership registration", body, [], false, contacts.DeveloperEmail);
                }
            }

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

            if (contacts.RegistrationContacts is not null)
            {
                foreach (var registrationContact in contacts.RegistrationContacts)
                {
                    await emailProvider.SendAsync(registrationContact.Name, registrationContact.Email, contacts.WebsiteFromName, contacts.WebsiteFromEmail, "Duplicate website membership registration", body, null, false, contacts.DeveloperEmail);
                }
            }

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
            await LogAuditAsync("EmailNotFound", model.Email, $"Login attempt for email '{model.Email}' failed as the email was not found.", false);
            return LoginResult.NotFound;
        }

        if (!member.IsApproved)
        {
            await LogAuditAsync("NotApproved", model.Email, $"Login attempt for email '{model.Email}' failed as the member is not approved.", false);
            return LoginResult.NotApproved;
        }

        if (!PasswordValidator.ComparePassword(model.Password, member.PasswordHash))
        {
            await LogAuditAsync("InvalidPassword", model.Email, $"Login attempt for email '{model.Email}' failed as the password was incorrect.", false);
            return LoginResult.InvalidPassword;
        }

        await SignInAsync(member);

        if (member.IsAdmin)
        {
            await LogAuditAsync("AdminLogin", model.Email, $"Login attempt for email '{model.Email}' was successful as admin user.", true);
            return LoginResult.Administrator;
        }

        await LogAuditAsync("Login", model.Email, $"Login attempt for email '{model.Email}' was successful as normal user.", true);
        return LoginResult.Success;
    }

    private async Task LogAuditAsync(string eventName, string email, string payload, bool success)
    {
        var auditLog = auditLogDal.Add();

        auditLog.Email = email;
        auditLog.EventName = eventName;
        auditLog.Payload = payload;
        auditLog.Success = success;

        auditLogDal.SweepOldRecords();

        await memberDal.CommitAsync();
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
            IsPersistent = false,
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
        member.PasswordResetTokenExpiry = DateTimeOffset.Now.AddDays(appSettings.PasswordResetTokenExpiryDays);
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
        var member = await memberDal.ByPasswordResetTokenAsync(model.PasswordResetToken);

        if (member is null)
        {
            AppLogging.Write($"Password reset token does not link to member. Token passed was {model.PasswordResetToken}.");
            return false;
        }

        if (!PasswordResetGuidIsValid(member))
        {
            AppLogging.Write($"Password reset token is invalid. Token passed was {model.PasswordResetToken}.");
            return false;
        }

        member.PasswordResetToken = null;
        member.PasswordResetTokenExpiry = null;
        member.PasswordHash = PasswordValidator.HashPassword(model.Password);

        await memberDal.CommitAsync();

        await SignInAsync(member);

        return true;
    }

    public async Task<bool> ValidatePasswordResetToken(Guid passwordResetToken)
    {
        var member = await memberDal.ByPasswordResetTokenAsync(passwordResetToken);

        return PasswordResetGuidIsValid(member);
    }

    private bool PasswordResetGuidIsValid(Member? member)
    {
        if (member is null)
        {
            return false;
        }

        if (member.PasswordResetTokenExpiry is null)
        {
            return false;
        }

        return member.PasswordResetTokenExpiry >= timeProvider.GetUtcNow();
    }
}
