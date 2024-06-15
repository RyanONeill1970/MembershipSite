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

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error"); // TODO: Check.
        }

        // Run migrations before we start accepting connections.
        using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<MembershipContext>();
            await context.Database.MigrateAsync();
        }

        app.UseAuthentication();

        app.UseHttpsRedirection();

        // If requester does not specify a file, use index.html
        var options = new DefaultFilesOptions();
        options.DefaultFileNames.Clear();
        options.DefaultFileNames.Add("index.html");
        app.UseDefaultFiles(options);

        app.UseStaticFiles(); // For normal wwwroot files accessible publicly because we are registered before UseAuthorization.

        app.UseRouting();

        // Everything after this point is secured implicitly unless opted out (via attributes on action).
        app.UseAuthorization();

        // Keep the 'secure' static files out of wwwroot so they can be locked down. Must be after UseAuthorization.
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "secure")),
            RequestPath = "/secure"
        });

        app.MapControllerRoute( // TODO: Can we delete?
            name: "default",
            pattern: "{controller=StaticContent}/{action=Indexz}/{id?}");

        await app.RunAsync();
    }
}
