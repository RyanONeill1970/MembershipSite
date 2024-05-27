namespace MembershipSite.Logic.ExtensionMethods;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebsiteServices(this IServiceCollection services, AppSettings appSettings)
    {
        services
            .AddDatalayer()
            .AddSingleton(appSettings)
            .AddSingleton(appSettings.EmailConfig)
            .AddTransient<AuthService>()
            .AddTransient<IEmailProvider, MailgunProvider>()
            .AddTransient<MemberAdminService>();

        return services;
    }
}
