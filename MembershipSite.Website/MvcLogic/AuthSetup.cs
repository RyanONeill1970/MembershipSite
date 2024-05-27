namespace MembershipSite.Website.MvcLogic;

public static class AuthSetup
{
    public static void AddAuthenticationScheme(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddAuthentication()
            .AddCookie(options =>
            {
                options.AccessDeniedPath = "/auth/access-denied";
                options.Cookie = new CookieBuilder
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Name = "a", // Obfuscated auth cookie name, just not to be obvious.
                    Path = "/",
                    SameSite = SameSiteMode.Strict,
                    SecurePolicy = CookieSecurePolicy.Always,
                };
                options.ExpireTimeSpan = TimeSpan.FromDays(60);
                options.LoginPath = "/auth/login";
                options.LogoutPath = "/auth/logout";
                options.ReturnUrlParameter = "r"; // Smarter version of returnUrl, keeps things a bit neater.
                options.SlidingExpiration = true;
            });
    }
}
