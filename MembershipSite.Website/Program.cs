namespace MembershipSite.Website;

using Microsoft.AspNetCore.DataProtection;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var appSettings = builder.Configuration
            .GetRequiredSection("AppSettings")
            .Get<AppSettings>();

        appSettings ??= new AppSettings();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services
            .AddDbContext<MembershipContext>(options => options.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure()))
            .AddHttpContextAccessor()
            .AddWebsiteServices(appSettings)
            .AddControllersWithViews()
            .AddRazorOptions(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeViewLocator());
            })
            .AddRazorRuntimeCompilation();

        builder.Services
            .AddDataProtection()
            .PersistKeysToDbContext<MembershipContext>()
            .SetApplicationName(appSettings.ApplicationName ?? "MembershipWebsite")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(180)); // This helps members who infrequently login.

        // Enabling error logging and performance monitoring. Settings held in appsettings.
        builder.WebHost.UseSentry();

        builder.AddAuthenticationScheme();

        // This enables securing of the static HTML files in the secure folder.
        // The static HTML files will 'fall back' to this authorisation method.
        // Actions and controllers will have their own explicitly defined security.
        builder.Services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build());

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
        }
        app.UseStatusCodePagesWithReExecute("/error", "?statusCode={0}");

        // Run migrations before we start accepting connections.
        using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<MembershipContext>();
            await context.Database.MigrateAsync();
        }

        // TODO: Re-enable.
        //app.UseHttpsRedirection();

        // If requester does not specify a file, use index.html
        var options = new DefaultFilesOptions();
        options.DefaultFileNames.Clear();
        options.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(options);

        app.UseMiddleware<LoginLinksMiddleware>();

        app.UseRouting();

        // Everything after this point is secured implicitly unless opted out (via attributes on action).
        app.UseAuthentication();
        app.UseStaticFiles(); // For normal wwwroot files accessible publicly because we are registered before UseAuthorization.
        app.UseAuthorization();

        // Turn "/secure" (url) into "secure" for local paths.
        var normalisedSecurePath = appSettings.SecureAreaRoot.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var secureDefaultFileOptions = new DefaultFilesOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, normalisedSecurePath)),
            RequestPath = appSettings.SecureAreaRoot,
        };
        secureDefaultFileOptions.RequestPath = appSettings.SecureAreaRoot;
        secureDefaultFileOptions.DefaultFileNames.Clear();
        secureDefaultFileOptions.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(secureDefaultFileOptions);

        // Keep the 'secure' static files out of wwwroot so they can be locked down. Must be after UseAuthorization.
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = secureDefaultFileOptions.FileProvider,
            OnPrepareResponse = ctx =>
            {
                // Prevent browser caching of anything in the secure directory which ensures when the user logs out they can't access a browser version of the page.
                ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                ctx.Context.Response.Headers.Append("Pragma", "no-cache");
                ctx.Context.Response.Headers.Append("Expires", "-1");
            },
            RequestPath = appSettings.SecureAreaRoot,
        });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        await app.RunAsync();
    }
}
